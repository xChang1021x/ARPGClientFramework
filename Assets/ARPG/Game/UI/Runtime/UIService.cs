using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.UI
{
    /// <summary>
    /// UI运行时管理服务。
    ///
    /// 负责：
    /// 1. UI异步创建；
    /// 2. 并发Open请求合并；
    /// 3. UI实例生命周期；
    /// 4. UI资源所有权；
    /// 5. Requested State；
    /// 6. UI Navigation；
    /// 7. Back行为。
    /// </summary>
    public sealed class UIService
        : IShutdownable
    {
        private readonly IResourceService _resourceService;
        private readonly UIRoot _uiRoot;

        /// <summary>
        /// 已经创建完成的UI。
        /// </summary>
        private readonly Dictionary<Type, UIEntry>
            _entries = new();

        /// <summary>
        /// 正在创建中的UI共享任务。
        /// </summary>
        private readonly Dictionary<Type, Task<UIPanel>>
            _openingTasks = new();

        /// <summary>
        /// 业务对UI最新的生命周期意图。
        /// </summary>
        private readonly Dictionary<Type, UIRequestedState>
            _requestedStates = new();

        /// <summary>
        /// UI返回导航顺序。
        ///
        /// index 0：
        /// 最底部。
        ///
        /// Count - 1：
        /// 当前Back应优先关闭的UI。
        ///
        /// 使用List而不是Stack，
        /// 是为了支持从中间移除UI。
        /// </summary>
        private readonly List<Type>
            _navigationStack = new();

        private bool _isShutdown;

        public UIService(
            IResourceService resourceService,
            UIRoot uiRoot)
        {
            _resourceService =
                resourceService
                ?? throw new ArgumentNullException(
                    nameof(resourceService));

            _uiRoot =
                uiRoot
                    ? uiRoot
                    : throw new ArgumentNullException(
                        nameof(uiRoot));
        }

        /// <summary>
        /// 打开指定UI。
        ///
        /// 业务层只需要知道Panel类型，
        /// Address / Layer / Navigation策略
        /// 全部由UIRegistry提供。
        /// </summary>
        public async Task<TPanel> OpenAsync<TPanel>(
            CancellationToken cancellationToken = default)
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            cancellationToken
                .ThrowIfCancellationRequested();

            Type panelType =
                typeof(TPanel);

            UIConfig config =
                UIRegistry.Get<TPanel>();

            /*
             * Latest Intent。
             */
            _requestedStates[panelType] =
                UIRequestedState.Open;

            /*
             * 已经创建完成：
             * 不重新加载、不重新实例化。
             */
            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry existingEntry))
            {
                OpenPanel(
                    panelType,
                    existingEntry);

                return (TPanel)existingEntry.Panel;
            }

            /*
             * 尚未创建。
             *
             * 如果已经存在共享Opening Task：
             * 复用。
             *
             * 如果不存在：
             * 创建并注册占位Task。
             */
            if (!_openingTasks.TryGetValue(
                    panelType,
                    out Task<UIPanel> openingTask))
            {
                var completionSource =
                    new TaskCompletionSource<UIPanel>();

                /*
                 * 先注册in-flight状态，
                 * 再真正启动异步工作。
                 *
                 * 避免同步完成造成重入竞态。
                 */
                openingTask =
                    completionSource.Task;

                _openingTasks.Add(
                    panelType,
                    openingTask);

                _ = RunOpenInternalAsync<TPanel>(
                    config,
                    panelType,
                    completionSource);
            }

            UIPanel panel =
                await openingTask;

            /*
             * 这里只取消当前调用方消费结果。
             * 不取消共享创建流程。
             */
            cancellationToken
                .ThrowIfCancellationRequested();

            ThrowIfShutdown();

            /*
             * 根据最新Requested State决定最终状态。
             */
            ApplyRequestedState(
                panelType,
                panel);

            return (TPanel)panel;
        }

        /// <summary>
        /// 协调共享UI创建任务。
        /// </summary>
        private async Task RunOpenInternalAsync<TPanel>(
            UIConfig config,
            Type panelType,
            TaskCompletionSource<UIPanel> completionSource)
            where TPanel : UIPanel
        {
            try
            {
                UIPanel panel =
                    await OpenInternalAsync<TPanel>(
                        config);

                completionSource.SetResult(
                    panel);
            }
            catch (Exception exception)
            {
                completionSource.SetException(
                    exception);
            }
            finally
            {
                _openingTasks.Remove(
                    panelType);
            }
        }

        /// <summary>
        /// 真正执行UI创建：
        ///
        /// Load
        /// → Instantiate
        /// → Validate
        /// → InitializeClosed
        /// → Register UIEntry
        /// → Ownership Transfer
        /// </summary>
        private async Task<UIPanel> OpenInternalAsync<TPanel>(
            UIConfig config)
            where TPanel : UIPanel
        {
            Type panelType =
                typeof(TPanel);

            ResourceHandle<GameObject> resourceHandle =
                null;

            GameObject instance =
                null;

            try
            {
                resourceHandle =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            config.Address);

                ThrowIfShutdown();

                Transform parent =
                    _uiRoot.GetLayerRoot(
                        config.Layer);

                instance =
                    UnityEngine.Object.Instantiate(
                        resourceHandle.Asset,
                        parent,
                        false);

                TPanel panel =
                    instance.GetComponent<TPanel>();

                if (panel == null)
                {
                    throw new InvalidOperationException(
                        $"UI prefab '{config.Address}' does not " +
                        $"contain component '{panelType.Name}'.");
                }

                /*
                 * 创建完成后的统一基线状态：
                 *
                 * Closed + inactive。
                 */
                panel.InitializeClosed();

                var entry =
                    new UIEntry(
                        panel,
                        resourceHandle,
                        config);

                _entries.Add(
                    panelType,
                    entry);

                /*
                 * ownership transfer：
                 *
                 * ResourceHandle
                 * OpenInternalAsync → UIEntry
                 */
                resourceHandle = null;

                /*
                 * GameObject实例
                 * OpenInternalAsync → UIService
                 */
                instance = null;

                return panel;
            }
            catch
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(
                        instance);
                }

                resourceHandle?.Dispose();

                throw;
            }
        }

        /// <summary>
        /// 根据业务最新意图应用实际UI状态。
        /// </summary>
        private void ApplyRequestedState(
            Type panelType,
            UIPanel panel)
        {
            if (!_requestedStates.TryGetValue(
                    panelType,
                    out UIRequestedState requestedState))
            {
                return;
            }

            switch (requestedState)
            {
                case UIRequestedState.Open:
                    {
                        if (_entries.TryGetValue(
                                panelType,
                                out UIEntry entry))
                        {
                            OpenPanel(
                                panelType,
                                entry);
                        }

                        break;
                    }

                case UIRequestedState.Closed:
                    {
                        ClosePanel(
                            panelType,
                            panel);

                        break;
                    }

                case UIRequestedState.Destroyed:
                    {
                        if (_entries.TryGetValue(
                                panelType,
                                out UIEntry entry))
                        {
                            DestroyEntry(
                                panelType,
                                entry);
                        }

                        break;
                    }

                default:
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(requestedState),
                            requestedState,
                            null);
                    }
            }
        }

        /// <summary>
        /// 统一执行Panel打开行为。
        ///
        /// 除了UIPanel.Open之外，
        /// 还负责：
        /// 1. 同Layer提升Sibling顺序；
        /// 2. 更新Navigation Stack。
        /// </summary>
        private void OpenPanel(
            Type panelType,
            UIEntry entry)
        {
            entry.Panel.Open();

            /*
             * 对同一个Canvas Root下的UI，
             * 最后Sibling优先绘制在上方。
             */
            entry.Panel.transform
                .SetAsLastSibling();

            if (!entry.Config.ParticipateInNavigation)
            {
                return;
            }

            /*
             * 防止同一个单例Panel
             * 在导航列表重复出现。
             *
             * 重新Open已有Panel时，
             * 相当于把它提升到导航顶部。
             */
            _navigationStack.Remove(
                panelType);

            _navigationStack.Add(
                panelType);
        }

        /// <summary>
        /// 统一执行Panel关闭行为。
        ///
        /// Close：
        /// 只隐藏实例，
        /// 不释放Prefab资源。
        ///
        /// 同时退出当前导航。
        /// </summary>
        private void ClosePanel(
            Type panelType,
            UIPanel panel)
        {
            panel.Close();

            _navigationStack.Remove(
                panelType);
        }

        /// <summary>
        /// 尝试获取已经创建完成的UI。
        /// </summary>
        public bool TryGet<TPanel>(
            out TPanel panel)
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            if (_entries.TryGetValue(
                    typeof(TPanel),
                    out UIEntry entry))
            {
                panel =
                    (TPanel)entry.Panel;

                return true;
            }

            panel = null;
            return false;
        }

        /// <summary>
        /// 关闭指定UI。
        ///
        /// 如果仍在Loading：
        /// 只记录Closed意图。
        ///
        /// 创建完成以后会根据RequestedState保持关闭。
        /// </summary>
        public void Close<TPanel>()
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            Type panelType =
                typeof(TPanel);

            _requestedStates[panelType] =
                UIRequestedState.Closed;

            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry entry))
            {
                ClosePanel(
                    panelType,
                    entry.Panel);
            }
        }

        /// <summary>
        /// 销毁指定UI实例，并释放它持有的Prefab资源所有权。
        /// </summary>
        public void Destroy<TPanel>()
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            Type panelType =
                typeof(TPanel);

            _requestedStates[panelType] =
                UIRequestedState.Destroyed;

            if (!_entries.TryGetValue(
                    panelType,
                    out UIEntry entry))
            {
                /*
                 * 如果仍在Loading，
                 * 保留Destroyed RequestedState。
                 *
                 * 创建完成后ApplyRequestedState会真正销毁。
                 */
                return;
            }

            DestroyEntry(
                panelType,
                entry);
        }

        /// <summary>
        /// 处理一次UI返回操作。
        ///
        /// 返回：
        /// true  = 成功关闭一个可导航UI。
        /// false = 当前没有可返回UI。
        /// </summary>
        public bool Back()
        {
            ThrowIfShutdown();

            while (_navigationStack.Count > 0)
            {
                int lastIndex =
                    _navigationStack.Count - 1;

                Type panelType =
                    _navigationStack[lastIndex];

                /*
                 * 如果发现stale记录，
                 * 先清理，再继续寻找下一个。
                 */
                if (!_entries.TryGetValue(
                        panelType,
                        out UIEntry entry))
                {
                    _navigationStack.RemoveAt(
                        lastIndex);

                    continue;
                }

                if (!entry.Panel.IsOpen)
                {
                    _navigationStack.RemoveAt(
                        lastIndex);

                    continue;
                }

                _requestedStates[panelType] =
                    UIRequestedState.Closed;

                ClosePanel(
                    panelType,
                    entry.Panel);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 真正销毁UIEntry。
        ///
        /// 顺序：
        /// Registry removal
        /// → Navigation removal
        /// → Instance Destroy
        /// → ResourceHandle Dispose
        /// → Requested State cleanup
        /// </summary>
        private void DestroyEntry(
            Type panelType,
            UIEntry entry)
        {
            _entries.Remove(
                panelType);

            _navigationStack.Remove(
                panelType);

            if (entry.Panel != null)
            {
                UnityEngine.Object.Destroy(
                    entry.Panel.gameObject);
            }

            entry.ResourceHandle.Dispose();

            /*
             * 真正Destroyed完成以后，
             * 当前UI已经不再有有效Requested State。
             */
            _requestedStates.Remove(
                panelType);
        }

        public void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;

            /*
             * 这里不能调用DestroyEntry，
             * 因为遍历Dictionary过程中修改Dictionary
             * 会导致枚举异常。
             */
            foreach (KeyValuePair<Type, UIEntry> pair
                     in _entries)
            {
                UIEntry entry =
                    pair.Value;

                if (entry.Panel != null)
                {
                    UnityEngine.Object.Destroy(
                        entry.Panel.gameObject);
                }

                entry.ResourceHandle.Dispose();
            }

            _entries.Clear();

            /*
             * Clear不等于取消实际异步工作。
             *
             * 尚未完成的OpenInternalAsync最终会在
             * ThrowIfShutdown后进入异常cleanup。
             */
            _openingTasks.Clear();

            _requestedStates.Clear();
            _navigationStack.Clear();
        }

        private void ThrowIfShutdown()
        {
            if (_isShutdown)
            {
                throw new ObjectDisposedException(
                    nameof(UIService));
            }
        }
    }
}
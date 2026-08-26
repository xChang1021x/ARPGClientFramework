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
    /// 2. 相同UI并发Open请求合并；
    /// 3. UI实例生命周期；
    /// 4. UI Prefab资源所有权；
    /// 5. 记录并应用最新UI生命周期意图。
    /// </summary>
    public sealed class UIService
        : IShutdownable
    {
        private readonly IResourceService _resourceService;
        private readonly UIRoot _uiRoot;

        /// <summary>
        /// 已创建完成的UI实例。
        /// </summary>
        private readonly Dictionary<Type, UIEntry>
            _entries = new();

        /// <summary>
        /// 当前正在创建中的UI。
        ///
        /// 用于合并相同Panel类型的并发Open请求。
        /// </summary>
        private readonly Dictionary<Type, Task<UIPanel>>
            _openingTasks = new();

        /// <summary>
        /// 业务对每个UI最新的生命周期意图。
        ///
        /// 异步创建完成后，不执行旧Open命令，
        /// 而是应用这里保存的最新状态。
        /// </summary>
        private readonly Dictionary<Type, UIRequestedState>
            _requestedStates = new();

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
        /// 如果已经存在：
        /// 直接打开已有实例。
        ///
        /// 如果正在创建：
        /// 共享同一个Opening Task。
        ///
        /// 如果不存在：
        /// 发起一次新的异步创建。
        /// </summary>
        public async Task<TPanel> OpenAsync<TPanel>(
            string address,
            UILayer layer,
            CancellationToken cancellationToken = default)
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            cancellationToken
                .ThrowIfCancellationRequested();

            Type panelType =
                typeof(TPanel);

            /*
             * Latest Intent：
             * 当前业务最新希望这个UI最终处于Open状态。
             */
            _requestedStates[panelType] =
                UIRequestedState.Open;

            /*
             * UI已经创建完成。
             */
            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry existingEntry))
            {
                existingEntry.Panel.Open();

                return (TPanel)existingEntry.Panel;
            }

            /*
             * UI尚未完成创建。
             *
             * 若已有Opening Task，则复用。
             * 否则先注册占位Task，再启动创建流程。
             */
            if (!_openingTasks.TryGetValue(
                    panelType,
                    out Task<UIPanel> openingTask))
            {
                var completionSource =
                    new TaskCompletionSource<UIPanel>();

                /*
                 * 非常重要：
                 *
                 * 必须先把Task注册进_openingTasks，
                 * 再启动真正的异步创建。
                 *
                 * 防止底层操作同步完成时产生重入时序问题。
                 */
                openingTask =
                    completionSource.Task;

                _openingTasks.Add(
                    panelType,
                    openingTask);

                _ = RunOpenInternalAsync<TPanel>(
                    address,
                    layer,
                    panelType,
                    completionSource);
            }

            UIPanel panel =
                await openingTask;

            /*
             * CancellationToken只取消当前调用方
             * 对最终结果的消费。
             *
             * 不取消共享的UI创建任务。
             */
            cancellationToken
                .ThrowIfCancellationRequested();

            ThrowIfShutdown();

            /*
             * 不执行最初的Open命令，
             * 而是应用业务最新意图。
             */
            ApplyRequestedState(
                panelType,
                panel);

            return (TPanel)panel;
        }

        /// <summary>
        /// 共享UI创建任务的协调层。
        ///
        /// 负责：
        /// 1. 将创建结果写入TaskCompletionSource；
        /// 2. 传播异常；
        /// 3. 清除opening状态。
        ///
        /// OpenInternalAsync本身不应该操作_openingTasks。
        /// </summary>
        private async Task RunOpenInternalAsync<TPanel>(
            string address,
            UILayer layer,
            Type panelType,
            TaskCompletionSource<UIPanel> completionSource)
            where TPanel : UIPanel
        {
            try
            {
                UIPanel panel =
                    await OpenInternalAsync<TPanel>(
                        address,
                        layer);

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
                /*
                 * opening生命周期只在协调层维护。
                 */
                _openingTasks.Remove(
                    panelType);
            }
        }

        /// <summary>
        /// 真正执行UI创建。
        ///
        /// 负责：
        /// Load Prefab
        /// → Instantiate
        /// → Validate Component
        /// → Initialize
        /// → Register UIEntry
        /// → Ownership Transfer
        /// </summary>
        private async Task<UIPanel> OpenInternalAsync<TPanel>(
            string address,
            UILayer layer)
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
                /*
                 * UI创建任务是共享任务。
                 *
                 * 单个Open调用方的CancellationToken
                 * 不能直接传递进共享Resource Load。
                 */
                resourceHandle =
                    await _resourceService
                        .LoadAsync<GameObject>(
                            address);

                ThrowIfShutdown();

                Transform parent =
                    _uiRoot.GetLayerRoot(
                        layer);

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
                        $"UI prefab '{address}' does not " +
                        $"contain component '{panelType.Name}'.");
                }

                /*
                 * 所有新创建Panel统一从Closed状态开始。
                 *
                 * 避免：
                 * IsOpen == false
                 * 但GameObject实际上Active
                 * 的状态不一致。
                 */
                panel.InitializeClosed();

                var entry =
                    new UIEntry(
                        panel,
                        resourceHandle,
                        layer);

                _entries.Add(
                    panelType,
                    entry);

                /*
                 * ResourceHandle ownership：
                 *
                 * OpenInternalAsync
                 *      ↓
                 * UIEntry / UIService
                 */
                resourceHandle = null;

                /*
                 * GameObject instance ownership：
                 *
                 * OpenInternalAsync
                 *      ↓
                 * UIService
                 */
                instance = null;

                return panel;
            }
            catch
            {
                /*
                 * Instantiate已经成功，
                 * 但后续步骤失败时，
                 * 必须销毁运行时实例。
                 */
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(
                        instance);
                }

                /*
                 * ResourceHandle还没有完成ownership transfer时，
                 * 当前方法仍然负责释放。
                 */
                resourceHandle?.Dispose();

                throw;
            }

            /*
             * 注意：
             *
             * 这里绝对不要再：
             *
             * finally
             * {
             *     _openingTasks.Remove(...);
             * }
             *
             * opening状态属于RunOpenInternalAsync协调层。
             */
        }

        /// <summary>
        /// 将业务最新意图应用到已经创建好的Panel。
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
                        panel.Open();
                        break;
                    }

                case UIRequestedState.Closed:
                    {
                        panel.Close();
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
        /// 请求关闭UI。
        ///
        /// 即使Panel仍处于异步创建阶段，
        /// Closed意图也会被记录下来。
        /// </summary>
        public void Close<TPanel>()
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            Type panelType =
                typeof(TPanel);

            _requestedStates[panelType] =
                UIRequestedState.Closed;

            /*
             * 如果Panel已经存在，
             * 立即应用Close。
             *
             * 如果还在Loading，
             * 等创建完成后ApplyRequestedState处理。
             */
            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry entry))
            {
                entry.Panel.Close();
            }
        }

        /// <summary>
        /// 请求销毁UI。
        ///
        /// 已创建：
        /// 立即销毁实例并释放资源Handle。
        ///
        /// 正在创建：
        /// 记录Destroyed意图，
        /// 创建完成后立即Destroy。
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
                return;
            }

            DestroyEntry(
                panelType,
                entry);
        }

        /// <summary>
        /// 真正销毁一个已经创建完成的UIEntry。
        /// </summary>
        private void DestroyEntry(
            Type panelType,
            UIEntry entry)
        {
            _entries.Remove(
                panelType);

            /*
             * 先销毁运行时GameObject实例。
             */
            if (entry.Panel != null)
            {
                UnityEngine.Object.Destroy(
                    entry.Panel.gameObject);
            }

            /*
             * 再释放Prefab资源所有权。
             */
            entry.ResourceHandle.Dispose();
        }

        public void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;

            /*
             * 统一销毁所有已经创建完成的UI实例，
             * 并释放它们持有的Prefab ResourceHandle。
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
             * Clear并不会真正取消已经运行中的Task。
             *
             * OpenInternalAsync加载完成后会通过
             * ThrowIfShutdown进入异常cleanup。
             */
            _openingTasks.Clear();

            _requestedStates.Clear();
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
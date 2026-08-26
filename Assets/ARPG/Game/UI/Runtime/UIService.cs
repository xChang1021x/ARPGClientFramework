using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.UI
{
    public sealed class UIService
        : IShutdownable
    {
        private readonly IResourceService _resourceService;
        private readonly UIRoot _uiRoot;

        private readonly Dictionary<Type, UIEntry>
            _entries = new();

        private readonly Dictionary<Type, Task<UIPanel>>
            _openingTasks = new();

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

        public async Task<TPanel> OpenAsync<TPanel>(
            string address,
            UILayer layer,
            CancellationToken cancellationToken = default)
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            cancellationToken.ThrowIfCancellationRequested();

            Type panelType =
                typeof(TPanel);

            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry existingEntry))
            {
                existingEntry.Panel.Open();

                return (TPanel)existingEntry.Panel;
            }

            if (!_openingTasks.TryGetValue(
                    panelType,
                    out Task<UIPanel> openingTask))
            {
                var completionSource =
                    new TaskCompletionSource<UIPanel>();

                /*
                 * 关键：
                 * 先把in-flight状态注册进去。
                 */
                openingTask =
                    completionSource.Task;

                _openingTasks.Add(
                    panelType,
                    openingTask);

                /*
                 * 注册完成后，再启动真正的创建流程。
                 */
                RunOpenInternalAsync<TPanel>(
                    address,
                    layer,
                    panelType,
                    completionSource);
            }

            UIPanel panel =
                await openingTask;

            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfShutdown();

            panel.Open();

            return (TPanel)panel;
        }

        private async void RunOpenInternalAsync<TPanel>(
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
                _openingTasks.Remove(
                    panelType);
            }
        }

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

                var entry =
                    new UIEntry(
                        panel,
                        resourceHandle,
                        layer);

                _entries.Add(
                    panelType,
                    entry);

                /*
                 * ownership：
                 *
                 * resourceHandle
                 * OpenInternalAsync → UIEntry
                 */
                resourceHandle = null;

                /*
                 * instance ownership也已经进入UIService。
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
            finally
            {
                _openingTasks.Remove(
                    panelType);
            }
        }

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

        public void Close<TPanel>()
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            if (!_entries.TryGetValue(
                    typeof(TPanel),
                    out UIEntry entry))
            {
                return;
            }

            entry.Panel.Close();
        }

        public void Destroy<TPanel>()
            where TPanel : UIPanel
        {
            ThrowIfShutdown();

            Type panelType =
                typeof(TPanel);

            if (!_entries.TryGetValue(
                    panelType,
                    out UIEntry entry))
            {
                return;
            }

            _entries.Remove(
                panelType);

            UnityEngine.Object.Destroy(
                entry.Panel.gameObject);

            entry.ResourceHandle.Dispose();
        }

        public void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;

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
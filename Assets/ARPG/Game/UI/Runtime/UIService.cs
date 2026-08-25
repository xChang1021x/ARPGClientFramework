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

            Type panelType =
                typeof(TPanel);

            if (_entries.TryGetValue(
                    panelType,
                    out UIEntry existingEntry))
            {
                existingEntry.Panel.Open();

                return (TPanel)existingEntry.Panel;
            }

            ResourceHandle<GameObject> resourceHandle =
                await _resourceService
                    .LoadAsync<GameObject>(
                        address,
                        cancellationToken);

            try
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                ThrowIfShutdown();

                Transform parent =
                    _uiRoot.GetLayerRoot(
                        layer);

                GameObject instance =
                    UnityEngine.Object.Instantiate(
                        resourceHandle.Asset,
                        parent,
                        false);

                TPanel panel =
                    instance.GetComponent<TPanel>();

                if (panel == null)
                {
                    UnityEngine.Object.Destroy(
                        instance);

                    throw new InvalidOperationException(
                        $"UI prefab '{address}' does not " +
                        $"contain component '{typeof(TPanel).Name}'.");
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
                 * ownership已经转移给UIEntry，
                 * 所以不能在finally里Dispose。
                 */
                resourceHandle = null;

                panel.Open();

                return panel;
            }
            finally
            {
                /*
                 * 如果实例化/校验过程中发生异常，
                 * ResourceHandle仍归当前方法所有，
                 * 必须释放。
                 */
                resourceHandle?.Dispose();
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
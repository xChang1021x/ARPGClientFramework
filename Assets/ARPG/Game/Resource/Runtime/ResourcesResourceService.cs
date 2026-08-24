using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Framework.Logging;
using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 基于Unity Resources API的资源服务实现。
    ///
    /// 主要用于：
    /// 1. 本地测试；
    /// 2. 简单资源加载；
    /// 3. 验证IResourceService抽象。
    ///
    /// 商业项目正式资源方案优先使用Addressables等系统。
    /// </summary>
    public sealed class ResourcesResourceService
        : IResourceService, IShutdownable
    {
        private readonly LogService _logService;

        /// <summary>
        /// 已加载完成的资源缓存。
        /// </summary>
        private readonly Dictionary<ResourceKey, ResourceEntry>
            _cache = new();

        /// <summary>
        /// 当前正在进行中的异步加载请求。
        ///
        /// 同一个ResourceKey只允许存在一个底层加载Task，
        /// 后续调用方共享同一个Task。
        /// </summary>
        private readonly Dictionary<ResourceKey, Task<ResourceEntry>>
            _loadingTasks = new();

        private bool _isShutdown;

        public ResourcesResourceService(
            LogService logService)
        {
            _logService =
                logService
                ?? throw new ArgumentNullException(
                    nameof(logService));
        }

        /// <summary>
        /// 当前缓存资源数量。
        /// 主要用于调试和测试。
        /// </summary>
        public int CachedResourceCount =>
            _cache.Count;

        /// <summary>
        /// 当前正在加载的资源数量。
        /// </summary>
        public int LoadingResourceCount =>
            _loadingTasks.Count;

        public ResourceHandle<T> Load<T>(
            string address)
            where T : UnityEngine.Object
        {
            ThrowIfShutdown();
            ValidateAddress(address);

            ResourceKey key =
                new ResourceKey(
                    typeof(T),
                    address);

            // 1. Cache hit
            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry cachedEntry))
            {
                return AcquireHandle<T>(
                    key,
                    cachedEntry);
            }

            // 2. 真正进行同步Resources.Load
            T asset =
                Resources.Load<T>(address);

            if (asset == null)
            {
                throw new ResourceLoadException(
                    typeof(T),
                    address);
            }

            /*
             * Resources版本暂时不主动调用UnloadAsset。
             *
             * 因此底层Release策略目前为空操作。
             * 逻辑缓存生命周期仍然由ReferenceCount管理。
             */
            var entry =
                new ResourceEntry(
                    asset,
                    releaseUnderlyingAction: () =>
                    {
                        // Intentionally no-op.
                    });

            _cache.Add(
                key,
                entry);

            _logService.Debug(
                "Resource",
                $"Loaded and cached resource '{key}'.");

            return AcquireHandle<T>(
                key,
                entry);
        }

        public async Task<ResourceHandle<T>> LoadAsync<T>(
            string address,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ThrowIfShutdown();
            ValidateAddress(address);

            /*
             * 在真正开始请求前先检查一次取消。
             */
            cancellationToken
                .ThrowIfCancellationRequested();

            ResourceKey key =
                new ResourceKey(
                    typeof(T),
                    address);

            ResourceEntry entry =
                await GetOrLoadEntryAsync<T>(
                    key);

            /*
             * 注意：
             * 底层加载请求可能被多个调用方共享。
             *
             * 因此这里的CancellationToken只表示：
             * 当前调用方不再获取ResourceHandle。
             *
             * 不直接取消共享加载Task。
             */
            cancellationToken
                .ThrowIfCancellationRequested();

            ThrowIfShutdown();

            return AcquireHandle<T>(
                key,
                entry);
        }

        /// <summary>
        /// 获取缓存条目；
        /// 若不存在则发起加载；
        /// 若已有相同in-flight请求则共享。
        /// </summary>
        private async Task<ResourceEntry> GetOrLoadEntryAsync<T>(
            ResourceKey key)
            where T : UnityEngine.Object
        {
            // 已加载完成。
            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry cachedEntry))
            {
                return cachedEntry;
            }

            // 查找是否已有相同资源正在加载。
            if (!_loadingTasks.TryGetValue(
                    key,
                    out Task<ResourceEntry> loadingTask))
            {
                loadingTask =
                    LoadEntryInternalAsync<T>(
                        key);

                _loadingTasks.Add(
                    key,
                    loadingTask);
            }

            try
            {
                ResourceEntry loadedEntry =
                    await loadingTask;

                /*
                 * Service在等待期间可能已经Shutdown。
                 *
                 * 此时不能再把完成的资源放回Cache。
                 */
                if (_isShutdown)
                {
                    loadedEntry.ReleaseUnderlying();

                    throw new ObjectDisposedException(
                        nameof(ResourcesResourceService));
                }

                /*
                 * 在异步等待期间，
                 * 理论上可能有同步加载把资源加入Cache。
                 *
                 * 防御性处理：
                 * 优先使用已经存在的Cache Entry。
                 */
                if (_cache.TryGetValue(
                        key,
                        out cachedEntry))
                {
                    if (!ReferenceEquals(
                            cachedEntry,
                            loadedEntry))
                    {
                        loadedEntry.ReleaseUnderlying();
                    }

                    return cachedEntry;
                }

                _cache.Add(
                    key,
                    loadedEntry);

                _logService.Debug(
                    "Resource",
                    $"Loaded and cached resource asynchronously '{key}'.");

                return loadedEntry;
            }
            finally
            {
                /*
                 * 无论成功还是异常，
                 * in-flight表都必须清理。
                 */
                _loadingTasks.Remove(key);
            }
        }

        /// <summary>
        /// 真正调用Unity Resources.LoadAsync的地方。
        ///
        /// 同一个ResourceKey通过Request Dedup
        /// 最终只会进入这里一次。
        /// </summary>
        private static async Task<ResourceEntry> LoadEntryInternalAsync<T>(
                ResourceKey key)
            where T : UnityEngine.Object
        {
            ResourceRequest request =
                Resources.LoadAsync<T>(
                    key.Path);

            while (!request.isDone)
            {
                await Task.Yield();
            }

            T asset =
                request.asset as T;

            if (asset == null)
            {
                throw new ResourceLoadException(
                    typeof(T),
                    key.Path);
            }

            return new ResourceEntry(
                asset,
                releaseUnderlyingAction: () =>
                {
                    /*
                     * Resources版本暂时不主动UnloadAsset。
                     */
                });
        }

        /// <summary>
        /// 为一个调用方获取资源所有权。
        /// </summary>
        private ResourceHandle<T> AcquireHandle<T>(
            ResourceKey key,
            ResourceEntry entry)
            where T : UnityEngine.Object
        {
            entry.ReferenceCount++;

            _logService.Debug(
                "Resource",
                $"Acquired resource '{key}', " +
                $"RefCount={entry.ReferenceCount}.");

            return new ResourceHandle<T>(
                (T)entry.Asset,
                () => Release(key));
        }

        /// <summary>
        /// 释放一个调用方的资源所有权。
        /// </summary>
        private void Release(
            ResourceKey key)
        {
            /*
             * Shutdown已经统一释放Cache后，
             * 旧Handle随后Dispose时允许安全返回。
             */
            if (_isShutdown)
            {
                return;
            }

            if (!_cache.TryGetValue(
                    key,
                    out ResourceEntry entry))
            {
                _logService.Warning(
                    "Resource",
                    $"Tried to release uncached resource '{key}'.");

                return;
            }

            entry.ReferenceCount--;

            if (entry.ReferenceCount < 0)
            {
                throw new InvalidOperationException(
                    $"Resource '{key}' reference count " +
                    "became negative.");
            }

            _logService.Debug(
                "Resource",
                $"Released resource '{key}', " +
                $"RefCount={entry.ReferenceCount}.");

            if (entry.ReferenceCount > 0)
            {
                return;
            }

            ReleaseEntry(
                key,
                entry);
        }

        /// <summary>
        /// 当最后一个业务引用释放时，
        /// 移除逻辑缓存并执行Provider底层释放策略。
        /// </summary>
        private void ReleaseEntry(
            ResourceKey key,
            ResourceEntry entry)
        {
            _cache.Remove(key);

            entry.ReleaseUnderlying();

            _logService.Debug(
                "Resource",
                $"Removed resource '{key}' from cache.");
        }

        public void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;

            /*
             * 如果这里发现ReferenceCount > 0，
             * 说明有调用方忘记Dispose ResourceHandle。
             */
            foreach (KeyValuePair<
                         ResourceKey,
                         ResourceEntry> pair
                     in _cache)
            {
                ResourceEntry entry =
                    pair.Value;

                if (entry.ReferenceCount > 0)
                {
                    _logService.Warning(
                        "Resource",
                        $"Resource '{pair.Key}' still has " +
                        $"{entry.ReferenceCount} active reference(s) " +
                        "during shutdown.");
                }

                entry.ReleaseUnderlying();
            }

            _cache.Clear();

            /*
             * Clear本身不会真正取消已经开始的Unity加载。
             *
             * in-flight Task完成后会通过_isShutdown检查，
             * 释放自身结果，不重新进入Cache。
             */
            _loadingTasks.Clear();
        }

        private static void ValidateAddress(
            string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException(
                    "Resource address cannot be empty.",
                    nameof(address));
            }
        }

        private void ThrowIfShutdown()
        {
            if (_isShutdown)
            {
                throw new ObjectDisposedException(
                    nameof(ResourcesResourceService));
            }
        }
    }
}
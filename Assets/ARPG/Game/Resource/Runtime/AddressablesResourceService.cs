using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Framework.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 基于Unity Addressables的资源服务实现。
    ///
    /// 提供：
    /// 1. 同步/异步资源加载；
    /// 2. Cache；
    /// 3. Reference Counting；
    /// 4. 相同异步请求合并；
    /// 5. ResourceHandle所有权；
    /// 6. Addressables.Release底层释放。
    /// </summary>
    public sealed class AddressablesResourceService
        : IResourceService, IShutdownable
    {
        private readonly LogService _logService;

        /// <summary>
        /// 已完成资源缓存。
        /// </summary>
        private readonly Dictionary<ResourceKey, ResourceEntry>
            _cache = new();

        /// <summary>
        /// 当前正在执行的底层Addressables加载。
        ///
        /// 同一个ResourceKey只会存在一个Task。
        /// </summary>
        private readonly Dictionary<ResourceKey, Task<ResourceEntry>>
            _loadingTasks = new();

        private bool _isShutdown;

        public AddressablesResourceService(
            LogService logService)
        {
            _logService =
                logService
                ?? throw new ArgumentNullException(
                    nameof(logService));
        }

        public int CachedResourceCount =>
            _cache.Count;

        public int LoadingResourceCount =>
            _loadingTasks.Count;

        /// <summary>
        /// 同步加载Addressables资源。
        ///
        /// 注意：
        /// WaitForCompletion会阻塞当前线程。
        /// 正式游戏流程应优先使用LoadAsync。
        /// </summary>
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

            // Cache hit.
            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry cachedEntry))
            {
                return AcquireHandle<T>(
                    key,
                    cachedEntry);
            }

            AsyncOperationHandle<T> operationHandle =
                Addressables.LoadAssetAsync<T>(
                    address);

            bool ownershipTransferred = false;

            try
            {
                T asset =
                    operationHandle.WaitForCompletion();

                if (operationHandle.Status !=
                        AsyncOperationStatus.Succeeded ||
                    asset == null)
                {
                    throw new ResourceLoadException(
                        typeof(T),
                        address);
                }

                /*
                 * 将Addressables Handle的生命周期
                 * 转移给ResourceEntry。
                 */
                var entry =
                    new ResourceEntry(
                        asset,
                        () =>
                        {
                            if (operationHandle.IsValid())
                            {
                                Addressables.Release(
                                    operationHandle);
                            }
                        });

                ownershipTransferred = true;

                _cache.Add(
                    key,
                    entry);

                _logService.Debug(
                    "Resource",
                    $"Loaded Addressable resource '{key}'.");

                return AcquireHandle<T>(
                    key,
                    entry);
            }
            catch
            {
                /*
                 * 如果ResourceEntry尚未接管底层Handle，
                 * 当前方法必须负责Release。
                 */
                if (!ownershipTransferred &&
                    operationHandle.IsValid())
                {
                    Addressables.Release(
                        operationHandle);
                }

                throw;
            }
        }

        public async Task<ResourceHandle<T>> LoadAsync<T>(
            string address,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            ThrowIfShutdown();
            ValidateAddress(address);

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
             * Cancellation只取消当前调用方取得ownership。
             *
             * 不能直接Release共享Addressables请求，
             * 因为其他调用方可能还在等待同一个Task。
             */
            cancellationToken
                .ThrowIfCancellationRequested();

            ThrowIfShutdown();

            return AcquireHandle<T>(
                key,
                entry);
        }

        /// <summary>
        /// 获取资源缓存条目。
        ///
        /// 若资源正在加载，
        /// 后续调用方共享同一个Task。
        /// </summary>
        private async Task<ResourceEntry> GetOrLoadEntryAsync<T>(
            ResourceKey key)
            where T : UnityEngine.Object
        {
            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry cachedEntry))
            {
                return cachedEntry;
            }

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
                 * 加载期间整个ResourceService可能已Shutdown。
                 *
                 * 此时底层Addressables Handle必须被释放，
                 * 不能再写回Cache。
                 */
                if (_isShutdown)
                {
                    loadedEntry.ReleaseUnderlying();

                    throw new ObjectDisposedException(
                        nameof(AddressablesResourceService));
                }

                /*
                 * 防御性处理：
                 * 异步过程中可能有同步Load写入同一Cache。
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
                    $"Loaded Addressable resource asynchronously '{key}'.");

                return loadedEntry;
            }
            finally
            {
                _loadingTasks.Remove(key);
            }
        }

        /// <summary>
        /// 真正发起Addressables加载操作。
        ///
        /// Request Dedup之后，
        /// 同一个ResourceKey只会调用此方法一次。
        /// </summary>
        private static async Task<ResourceEntry>
            LoadEntryInternalAsync<T>(
                ResourceKey key)
            where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> operationHandle =
                Addressables.LoadAssetAsync<T>(
                    key.Path);

            bool ownershipTransferred = false;

            try
            {
                T asset =
                    await operationHandle.Task;

                if (operationHandle.Status !=
                        AsyncOperationStatus.Succeeded ||
                    asset == null)
                {
                    throw new ResourceLoadException(
                        typeof(T),
                        key.Path);
                }

                /*
                 * ResourceEntry接管Addressables Handle。
                 *
                 * 最后一个业务ResourceHandle释放时，
                 * ResourceEntry才会调用Addressables.Release。
                 */
                var entry =
                    new ResourceEntry(
                        asset,
                        () =>
                        {
                            if (operationHandle.IsValid())
                            {
                                Addressables.Release(
                                    operationHandle);
                            }
                        });

                ownershipTransferred = true;

                return entry;
            }
            catch
            {
                /*
                 * 加载失败时必须释放底层Handle。
                 *
                 * 这是Addressables资源管理非常重要的一条。
                 */
                if (!ownershipTransferred &&
                    operationHandle.IsValid())
                {
                    Addressables.Release(
                        operationHandle);
                }

                throw;
            }
        }

        /// <summary>
        /// 为当前调用方增加一次业务资源所有权。
        /// </summary>
        private ResourceHandle<T> AcquireHandle<T>(
            ResourceKey key,
            ResourceEntry entry)
            where T : UnityEngine.Object
        {
            entry.ReferenceCount++;

            _logService.Debug(
                "Resource",
                $"Acquired Addressable resource '{key}', " +
                $"RefCount={entry.ReferenceCount}.");

            return new ResourceHandle<T>(
                (T)entry.Asset,
                () => Release(key));
        }

        /// <summary>
        /// 当前调用方释放资源所有权。
        /// </summary>
        private void Release(
            ResourceKey key)
        {
            /*
             * ResourceService已经Shutdown后，
             * Cache和底层Handle都已统一处理。
             *
             * 旧ResourceHandle之后再Dispose，
             * 不应再次Release底层Handle。
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
                    $"Tried to release uncached " +
                    $"Addressable resource '{key}'.");

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
                $"Released Addressable resource '{key}', " +
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
        /// 最后一个业务引用释放：
        ///
        /// Cache Remove
        /// → ReleaseUnderlying
        /// → Addressables.Release
        /// </summary>
        private void ReleaseEntry(
            ResourceKey key,
            ResourceEntry entry)
        {
            _cache.Remove(key);

            entry.ReleaseUnderlying();

            _logService.Debug(
                "Resource",
                $"Released underlying Addressable resource '{key}'.");
        }

        public void Shutdown()
        {
            if (_isShutdown)
            {
                return;
            }

            _isShutdown = true;

            /*
             * ResourceService Shutdown时，
             * 即使业务Handle没有正确Dispose，
             * 也尽可能释放全部已经进入Cache的底层Handle。
             *
             * 同时通过Warning暴露资源所有权泄漏。
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
                        $"Addressable resource '{pair.Key}' still has " +
                        $"{entry.ReferenceCount} active reference(s) " +
                        "during shutdown.");
                }

                entry.ReleaseUnderlying();
            }

            _cache.Clear();

            /*
             * 清Dictionary不会真正取消已经开始的Addressables操作。
             *
             * 仍在运行的LoadEntryInternalAsync完成以后，
             * GetOrLoadEntryAsync会发现_isShutdown，
             * 随后ReleaseUnderlying。
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
                    nameof(AddressablesResourceService));
            }
        }
    }
}
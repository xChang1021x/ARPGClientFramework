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
    /// 基于Unity Resources API的资源加载实现。
    /// 当前用于验证资源服务架构，
    /// 后续可替换为Addressables等实现。
    /// </summary>
    public sealed class ResourcesResourceService
        : IResourceService, IShutdownable
    {
        private readonly LogService _logService;

        private readonly Dictionary<ResourceKey, ResourceEntry>
    _cache = new();

        private readonly Dictionary<ResourceKey, Task<UnityEngine.Object>>
    _loadingTasks = new();

        public ResourcesResourceService(
            LogService logService)
        {
            _logService =
                logService
                ?? throw new ArgumentNullException(
                    nameof(logService));
        }

        public T Load<T>(
            string path)
            where T : UnityEngine.Object
        {
            ValidatePath(path);

            T asset =
                Resources.Load<T>(path);

            if (asset == null)
            {
                throw new ResourceLoadException(
                    typeof(T),
                    path);
            }

            _logService.Debug(
                "Resource",
                $"Loaded resource '{path}' " +
                $"as '{typeof(T).Name}'.");

            return asset;
        }

        public bool TryLoad<T>(
            string path,
            out T asset)
            where T : UnityEngine.Object
        {
            ValidatePath(path);

            asset =
                Resources.Load<T>(path);

            if (asset != null)
            {
                return true;
            }

            _logService.Warning(
                "Resource",
                $"Resource '{path}' " +
                $"was not found as '{typeof(T).Name}'.");

            return false;
        }

        public async Task<T> LoadAsync<T>(
    string path,
    CancellationToken cancellationToken = default)
    where T : UnityEngine.Object
        {
            ValidatePath(path);

            ResourceRequest request =
                Resources.LoadAsync<T>(path);

            while (!request.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();

            T asset =
                request.asset as T;

            if (asset == null)
            {
                throw new ResourceLoadException(
                    typeof(T),
                    path);
            }

            _logService.Debug(
                "Resource",
                $"Loaded resource asynchronously '{path}' " +
                $"as '{typeof(T).Name}'.");

            return asset;
        }

        public ResourceHandle<T> LoadHandle<T>(
    string path)
    where T : UnityEngine.Object
        {
            ValidatePath(path);

            ResourceKey key =
                new ResourceKey(
                    typeof(T),
                    path);

            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry existingEntry))
            {
                existingEntry.ReferenceCount++;

                _logService.Debug(
                    "Resource",
                    $"Cache hit '{key}', " +
                    $"RefCount={existingEntry.ReferenceCount}.");

                return CreateHandle<T>(
                    key,
                    existingEntry);
            }

            T asset =
                Resources.Load<T>(path);

            if (asset == null)
            {
                throw new ResourceLoadException(
                    typeof(T),
                    path);
            }

            var entry =
                new ResourceEntry(asset)
                {
                    ReferenceCount = 1
                };

            _cache.Add(
                key,
                entry);

            _logService.Debug(
                "Resource",
                $"Cached '{key}', RefCount=1.");

            return CreateHandle<T>(
                key,
                entry);
        }

        private ResourceHandle<T> CreateHandle<T>(
    ResourceKey key,
    ResourceEntry entry)
    where T : UnityEngine.Object
        {
            return new ResourceHandle<T>(
                (T)entry.Asset,
                () => Release(key));
        }

        public async Task<ResourceHandle<T>>
    LoadHandleAsync<T>(
        string path,
        CancellationToken cancellationToken = default)
    where T : UnityEngine.Object
        {
            ValidatePath(path);

            cancellationToken.ThrowIfCancellationRequested();

            ResourceKey key =
                new ResourceKey(
                    typeof(T),
                    path);

            ResourceEntry entry =
                await GetOrLoadEntryAsync(
                    key);

            cancellationToken.ThrowIfCancellationRequested();

            entry.ReferenceCount++;

            _logService.Debug(
                "Resource",
                $"Acquired '{key}', " +
                $"RefCount={entry.ReferenceCount}.");

            return CreateHandle<T>(
                key,
                entry);
        }

        private async Task<UnityEngine.Object>
    LoadAssetInternalAsync(
        ResourceKey key)
        {
            ResourceRequest request =
                Resources.LoadAsync(
                    key.Path,
                    key.ResourceType);

            while (!request.isDone)
            {
                await Task.Yield();
            }

            UnityEngine.Object asset =
                request.asset;

            if (asset == null)
            {
                throw new ResourceLoadException(
                    key.ResourceType,
                    key.Path);
            }

            return asset;
        }

        private async Task<ResourceEntry>
    GetOrLoadEntryAsync(
        ResourceKey key)
        {
            if (_cache.TryGetValue(
                    key,
                    out ResourceEntry cachedEntry))
            {
                return cachedEntry;
            }

            if (!_loadingTasks.TryGetValue(
                    key,
                    out Task<UnityEngine.Object> loadingTask))
            {
                loadingTask =
                    LoadAssetInternalAsync(key);

                _loadingTasks.Add(
                    key,
                    loadingTask);
            }

            try
            {
                UnityEngine.Object asset =
                    await loadingTask;

                if (_cache.TryGetValue(
                        key,
                        out cachedEntry))
                {
                    return cachedEntry;
                }

                var newEntry =
                    new ResourceEntry(asset);

                _cache.Add(
                    key,
                    newEntry);

                return newEntry;
            }
            finally
            {
                _loadingTasks.Remove(key);
            }
        }

        private void Release<T>(
    T asset)
    where T : UnityEngine.Object
        {
            _logService.Debug(
                "Resource",
                $"Released resource handle for '{asset.name}'.");

            // Resources实现暂不主动UnloadAsset。
        }

        private void Release(
    ResourceKey key)
        {
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
                $"Released '{key}', " +
                $"RefCount={entry.ReferenceCount}.");

            if (entry.ReferenceCount > 0)
            {
                return;
            }

            ReleaseEntry(
                key,
                entry);
        }

        private void ReleaseEntry(
    ResourceKey key,
    ResourceEntry entry)
        {
            _cache.Remove(key);

            _logService.Debug(
                "Resource",
                $"Removed '{key}' from resource cache.");

            /*
             * Resources实现暂时不主动UnloadAsset。
             * 真正底层释放策略将在Addressables阶段实现。
             */
        }

        public void Shutdown()
        {
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
                        $"{entry.ReferenceCount} active references " +
                        "during shutdown.");
                }
            }

            _cache.Clear();
            _loadingTasks.Clear();
        }

        private static void ValidatePath(
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "Resource path cannot be empty.",
                    nameof(path));
            }
        }
    }
}
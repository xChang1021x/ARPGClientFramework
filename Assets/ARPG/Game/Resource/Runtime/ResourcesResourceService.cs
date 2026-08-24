using System;
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
            T asset =
                Load<T>(path);

            return new ResourceHandle<T>(
                asset,
                () => Release(asset));
        }

        public async Task<ResourceHandle<T>> LoadHandleAsync<T>(
    string path,
    CancellationToken cancellationToken = default)
    where T : UnityEngine.Object
        {
            T asset =
                await LoadAsync<T>(
                    path,
                    cancellationToken);

            return new ResourceHandle<T>(
                asset,
                () => Release(asset));
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

        public void Shutdown()
        {
            /*
             * 第一版暂时没有资源缓存，
             * 因此这里没有单独资源需要释放。
             *
             * 后续加入缓存/Addressables后，
             * Shutdown将承担统一Release职责。
             */
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
using System;
using System.Collections.Generic;

namespace ARPG.Framework.Core
{
    /// <summary>
    /// 游戏运行时服务容器。
    /// 负责服务注册、解析以及生命周期管理。
    /// </summary>
    public sealed class ServiceContainer : IDisposable
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly HashSet<object> _lifecycleInstances = new();
        private readonly List<IInitializable> _initializables = new();
        private readonly List<IShutdownable> _shutdownables = new();

        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// 注册服务。
        /// 每种注册类型只能存在一个实例。
        /// </summary>
        public void Register<TService>(
            TService service)
            where TService : class
        {
            ThrowIfDisposed();

            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot register services after initialization.");
            }

            if (service == null)
            {
                throw new ArgumentNullException(
                    nameof(service));
            }

            Type serviceType = typeof(TService);

            if (_services.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.Name}' " +
                    "has already been registered.");
            }

            _services.Add(
                serviceType,
                service);

            bool isNewLifecycleInstance = _lifecycleInstances.Add(service);

            if (isNewLifecycleInstance)
            {
                if (service is IInitializable initializable)
                {
                    _initializables.Add(initializable);
                }

                if (service is IShutdownable shutdownable)
                {
                    _shutdownables.Add(shutdownable);
                }
            }
        }

        /// <summary>
        /// 获取已注册服务。
        /// 服务不存在时直接抛出异常。
        /// </summary>
        public TService Get<TService>()
            where TService : class
        {
            ThrowIfDisposed();

            Type serviceType =
                typeof(TService);

            if (!_services.TryGetValue(
                    serviceType,
                    out object service))
            {
                throw new InvalidOperationException(
                    $"Service '{serviceType.Name}' " +
                    "has not been registered.");
            }

            return (TService)service;
        }

        /// <summary>
        /// 尝试获取服务。
        /// </summary>
        public bool TryGet<TService>(
            out TService service)
            where TService : class
        {
            ThrowIfDisposed();

            if (_services.TryGetValue(
                    typeof(TService),
                    out object storedService))
            {
                service = (TService)storedService;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// 按注册顺序初始化需要生命周期管理的服务。
        /// </summary>
        public void Initialize()
        {
            ThrowIfDisposed();

            if (_isInitialized)
            {
                return;
            }

            foreach (IInitializable initializable
                     in _initializables)
            {
                initializable.Initialize();
            }

            _isInitialized = true;
        }

        /// <summary>
        /// 按初始化顺序的反方向关闭服务。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            for (int index = _shutdownables.Count - 1;
                 index >= 0;
                 index--)
            {
                _shutdownables[index].Shutdown();
            }

            _initializables.Clear();
            _shutdownables.Clear();
            _services.Clear();

            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(ServiceContainer));
            }
        }
    }
}
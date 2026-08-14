using System;
using System.Collections.Generic;
using ARPG.Framework.Event;
using ARPG.Framework.Timer;
using ARPG.Framework.Logging;
using ARPG.Framework.Diagnostics;

namespace ARPG.Framework.Core
{
    /// <summary>
    /// 游戏运行时上下文。
    /// 负责持有和管理客户端基础服务。
    /// </summary>
    public sealed class GameContext : IDisposable
    {
        private readonly List<IGameService> _services = new();

        private bool _isInitialized;
        private bool _isDisposed;

        public LogService LogService { get; }

        public IExceptionReporter ExceptionReporter { get; }

        public EventBus EventBus { get; }

        public TimerService TimerService { get; }

        public GameContext(
            ILogger logger,
            LogLevel minimumLogLevel)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            LogService = new LogService(
                logger,
                minimumLogLevel);

            ExceptionReporter =
                new LoggingExceptionReporter(
                    LogService);

            EventBus = new EventBus(
                ExceptionReporter);

            TimerService = new TimerService(
                ExceptionReporter);
        }

        /// <summary>
        /// 按注册顺序初始化所有服务。
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            foreach (IGameService service in _services)
            {
                service.Initialize();
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

            for (int index = _services.Count - 1; index >= 0; index--)
            {
                _services[index].Shutdown();
            }

            TimerService.Dispose();
            EventBus.ClearAll();

            _services.Clear();

            _isDisposed = true;
        }

        public void RegisterService(IGameService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot register services after GameContext has initialized.");
            }

            if (_services.Contains(service))
            {
                throw new InvalidOperationException(
                    $"Service '{service.GetType().Name}' has already been registered.");
            }

            _services.Add(service);
        }
    }
}
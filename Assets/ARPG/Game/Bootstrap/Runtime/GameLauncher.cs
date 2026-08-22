using System;
using ARPG.Framework.Config;
using ARPG.Framework.Core;
using ARPG.Framework.Diagnostics;
using ARPG.Framework.Event;
using ARPG.Framework.Logging;
using ARPG.Framework.Timer;
using ARPG.Game.Config;
using ARPG.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using FrameworkLogger = ARPG.Framework.Logging.ILogger;

namespace ARPG.Game.Bootstrap
{
    /// <summary>
    /// ARPG客户端唯一启动入口。
    /// 负责创建上下文、注册服务并启动游戏流程。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameLauncher : MonoBehaviour
    {
        [SerializeField]
        private string _entrySceneName = "Main";

        [SerializeField]
        private GameConfigManifest _configManifest;

        private static GameLauncher _instance;

        public static GameLauncher Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new System.InvalidOperationException(
                        "GameLauncher has not been initialized.");
                }

                return _instance;
            }
        }

        private GameContext _gameContext;

        public GameContext GameContext
        {
            get
            {
                if (_gameContext == null)
                {
                    throw new System.InvalidOperationException(
                        "GameContext has not been initialized.");
                }

                return _gameContext;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);

            InitializeApplication();
        }

        private void InitializeApplication()
        {
            _gameContext =
                new GameContext();

            RegisterCoreServices();

            RegisterConfigs();

            CreateRuntimeDrivers();

            RegisterGameServices();

            _gameContext.Initialize();

            GetLogService().Info(
                "Bootstrap",
                "Game context initialized.");

            EnterGame();
        }

        private void RegisterCoreServices()
        {
            ServiceContainer services =
                _gameContext.Services;

            FrameworkLogger logger =
                new UnityLogger();

            LogLevel minimumLevel =
                Debug.isDebugBuild
                    ? LogLevel.Debug
                    : LogLevel.Warning;

            var logService =
                new LogService(
                    logger,
                    minimumLevel);

            var exceptionReporter =
                new LoggingExceptionReporter(
                    logService);

            var eventBus =
                new EventBus(
                    exceptionReporter);

            var timerService =
                new TimerService(
                    exceptionReporter);

            var configService =
                new ConfigService();

            services.Register(
                logService);

            services.Register<IExceptionReporter>(
                exceptionReporter);

            services.Register(
                eventBus);

            services.Register(
                timerService);

            services.Register(
                configService);
        }

        /// <summary>
        /// 按依赖顺序注册服务。
        /// 被其他服务依赖的模块必须先注册。
        /// </summary>
        private void RegisterGameServices()
        {
            ServiceContainer services =
                _gameContext.Services;

            ConfigService configService =
                services.Get<ConfigService>();

            var playerService =
                new PlayerService(
                    configService);

            services.Register<IPlayerService>(
                playerService);
        }

        private void EnterGame()
        {
            if (string.IsNullOrWhiteSpace(_entrySceneName))
            {
                throw new InvalidOperationException(
                    "Entry scene name cannot be empty.");
            }

            SceneManager.LoadScene(
                _entrySceneName);
        }

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            _gameContext?.Dispose();
            _gameContext = null;
            _instance = null;
        }

        private void CreateRuntimeDrivers()
        {
            TimerService timerService =
                _gameContext.Services
                    .Get<TimerService>();

            TimerDriver timerDriver =
                gameObject.AddComponent<TimerDriver>();

            timerDriver.Initialize(
                timerService);
        }

        private void RegisterConfigs()
        {
            if (_configManifest == null)
            {
                throw new InvalidOperationException(
                    "Game config manifest has not been assigned.");
            }

            ConfigService configService =
                _gameContext.Services
                    .Get<ConfigService>();

            _configManifest.RegisterAll(
                configService);

            GetLogService().Info(
                "Config",
                "Game configuration registered.");
        }

        private LogService GetLogService()
        {
            return _gameContext.Services.Get<LogService>();
        }
    }
}
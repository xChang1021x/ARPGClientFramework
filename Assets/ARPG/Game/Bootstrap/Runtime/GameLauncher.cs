using ARPG.Framework.Core;
using ARPG.Framework.Event;
using ARPG.Game.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARPG.Game.Bootstrap
{
    /// <summary>
    /// ARPG客户端唯一启动入口。
    /// 负责创建上下文、注册服务并启动游戏流程。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameLauncher : MonoBehaviour
    {
        private static GameLauncher _instance;

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
            _gameContext = new GameContext();

            RegisterServices(_gameContext);

            _gameContext.Initialize();

            TestEventBus();

            EnterGame();
        }

        /// <summary>
        /// 按依赖顺序注册服务。
        /// 被其他服务依赖的模块必须先注册。
        /// </summary>
        private static void RegisterServices(GameContext context)
        {
            /*
             * 当前Day2暂时没有其他服务。
             *
             * 后续会逐步添加：
             *
             * context.RegisterService(new ConfigService(...));
             * context.RegisterService(new ResourceService(...));
             * context.RegisterService(new NetworkService(...));
             * context.RegisterService(new UIService(...));
             */
        }

        private static void EnterGame()
        {
            const string mainSceneName = "Main";

            SceneManager.LoadScene(mainSceneName);
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

        private void TestEventBus()
        {
            EventBus eventBus = _gameContext.EventBus;

            eventBus.Subscribe<TestGameStartedEvent>(
                OnGameStarted);

            eventBus.Publish(
                new TestGameStartedEvent("ARPG Client Started"));
        }

        private static void OnGameStarted(
            TestGameStartedEvent eventData)
        {
            Debug.Log(eventData.Message);
        }
    }
}
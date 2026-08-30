using System;
using System.Threading;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.Resource;
using ARPG.Game.UI;
using ARPG.Game.UI.Main;
using UnityEngine;

namespace ARPG.Game.Tests.UI
{
    /// <summary>
    /// Day17 UI异步生命周期状态一致性测试。
    ///
    /// 使用DelayedResourceService人为控制资源加载时机，
    /// 从而稳定复现：
    ///
    /// Open -> Close while loading
    /// Open -> Close -> Open while loading
    /// Open -> Destroy while loading
    /// </summary>
    public sealed class UIAsyncStateTester
        : MonoBehaviour
    {
        [Header("Test Setup")]

        [SerializeField]
        private UIRoot _uiRoot;

        private IResourceService _realResourceService;

        private DelayedResourceService _delayedResourceService;

        private UIService _testUIService;

        private void Awake()
        {
            if (_uiRoot == null)
            {
                throw new InvalidOperationException(
                    "UIRoot has not been assigned.");
            }

            ServiceContainer services =
                GameLauncher.Instance
                    .GameContext
                    .Services;

            _realResourceService =
                services.Get<IResourceService>();

            CreateTestUIService();
        }

        private void Update()
        {
            /*
             * 1：
             * Open -> Close while loading
             */
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestOpenThenCloseAsync();
            }

            /*
             * 2：
             * Open -> Close -> Open while loading
             */
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestLatestOpenIntentWinsAsync();
            }

            /*
             * 3：
             * Open -> Destroy while loading
             */
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestDestroyWhileOpeningAsync();
            }

            /*
             * 4：
             * 正常Open
             */
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestNormalOpenAsync();
            }

            /*
             * 5：
             * 清理当前测试Panel。
             */
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ResetTestState();
            }
        }

        /// <summary>
        /// 测试：
        ///
        /// Open
        /// ↓
        /// Loading
        /// ↓
        /// Close
        /// ↓
        /// Load完成
        ///
        /// 最终必须Closed。
        /// </summary>
        private async void TestOpenThenCloseAsync()
        {
            ResetTestState();

            try
            {
                Debug.Log(
                    "[Day17] Test Open -> Close started.");

                _delayedResourceService.Block();

                Task<MainPanel> openTask =
                    _testUIService.OpenAsync<MainPanel>();

                /*
                 * 此时资源被DelayedResourceService阻塞，
                 * Panel一定还没有完成创建。
                 */
                _testUIService.Close<MainPanel>();

                /*
                 * 允许真正资源加载继续。
                 */
                _delayedResourceService.Release();

                MainPanel panel =
                    await openTask;

                bool passed =
                    !panel.IsOpen &&
                    !panel.gameObject.activeSelf;

                if (passed)
                {
                    Debug.Log(
                        "[Day17] PASS: " +
                        "Open -> Close finished Closed.");
                }
                else
                {
                    Debug.LogError(
                        "[Day17] FAIL: " +
                        $"IsOpen={panel.IsOpen}, " +
                        $"Active={panel.gameObject.activeSelf}.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 测试latest-intent-wins：
        ///
        /// Open
        /// ↓
        /// Close
        /// ↓
        /// Open
        /// ↓
        /// Load完成
        ///
        /// 最终必须Open。
        /// </summary>
        private async void TestLatestOpenIntentWinsAsync()
        {
            ResetTestState();

            try
            {
                Debug.Log(
                    "[Day17] " +
                    "Test Open -> Close -> Open started.");

                _delayedResourceService.Block();

                Task<MainPanel> firstOpenTask =
                    _testUIService.OpenAsync<MainPanel>();

                _testUIService.Close<MainPanel>();

                /*
                 * 第二次Open不会创建第二个UI，
                 * 应复用同一个opening task，
                 * 同时把RequestedState重新改成Open。
                 */
                Task<MainPanel> secondOpenTask =
                    _testUIService.OpenAsync<MainPanel>();

                _delayedResourceService.Release();

                MainPanel[] panels =
                    await Task.WhenAll(
                        firstOpenTask,
                        secondOpenTask);

                bool samePanel =
                    ReferenceEquals(
                        panels[0],
                        panels[1]);

                bool passed =
                    samePanel &&
                    panels[0].IsOpen &&
                    panels[0].gameObject.activeSelf;

                if (passed)
                {
                    Debug.Log(
                        "[Day17] PASS: " +
                        "Latest Open intent wins.");
                }
                else
                {
                    Debug.LogError(
                        "[Day17] FAIL: " +
                        $"SamePanel={samePanel}, " +
                        $"IsOpen={panels[0].IsOpen}, " +
                        $"Active={panels[0].gameObject.activeSelf}.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 测试：
        ///
        /// Open
        /// ↓
        /// Loading
        /// ↓
        /// Destroy
        /// ↓
        /// Load完成
        ///
        /// 最终UIService不能继续保存该Panel。
        /// </summary>
        private async void TestDestroyWhileOpeningAsync()
        {
            ResetTestState();

            try
            {
                Debug.Log(
                    "[Day17] " +
                    "Test Open -> Destroy started.");

                _delayedResourceService.Block();

                Task<MainPanel> openTask =
                    _testUIService.OpenAsync<MainPanel>();

                _testUIService.Destroy<MainPanel>();

                _delayedResourceService.Release();

                /*
                 * 当前UIService v1有一个已知语义：
                 *
                 * OpenAsync仍然可能返回一个随后被Destroy的Panel引用。
                 *
                 * 因此这里不要继续操作返回值，
                 * 我们只等待流程完成。
                 */
                await openTask;

                /*
                 * Destroy在Unity中通常延迟到帧末真正销毁，
                 * 但UIService的Entry必须已经立即移除。
                 */
                bool stillExists =
                    _testUIService.TryGet<MainPanel>(
                        out _);

                if (!stillExists)
                {
                    Debug.Log(
                        "[Day17] PASS: " +
                        "Destroyed panel was removed from UIService.");
                }
                else
                {
                    Debug.LogError(
                        "[Day17] FAIL: " +
                        "Destroyed panel is still registered.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 正常Open回归测试。
        /// </summary>
        private async void TestNormalOpenAsync()
        {
            ResetTestState();

            try
            {
                _delayedResourceService.Release();

                MainPanel panel =
                    await _testUIService
                        .OpenAsync<MainPanel>();

                bool passed =
                    panel.IsOpen &&
                    panel.gameObject.activeSelf;

                Debug.Log(
                    passed
                        ? "[Day17] PASS: Normal Open."
                        : "[Day17] FAIL: Normal Open.");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 每个测试前重新创建独立UIService，
        /// 避免上一测试的RequestedState影响下一测试。
        /// </summary>
        private void ResetTestState()
        {
            _testUIService?.Shutdown();

            CreateTestUIService();
        }

        private void CreateTestUIService()
        {
            _delayedResourceService =
                new DelayedResourceService(
                    _realResourceService);

            _testUIService =
                new UIService(
                    _delayedResourceService,
                    _uiRoot);
        }

        private void OnDestroy()
        {
            _testUIService?.Shutdown();
            _testUIService = null;
        }

        /// <summary>
        /// 测试专用IResourceService装饰器。
        ///
        /// 它不改变真实资源系统实现，
        /// 只在人为Gate处阻塞LoadAsync。
        ///
        /// 这样测试可以精确制造：
        /// UI处于Loading状态。
        /// </summary>
        private sealed class DelayedResourceService
    : IResourceService
        {
            private readonly IResourceService _inner;

            private TaskCompletionSource<bool> _gate;

            public DelayedResourceService(
                IResourceService inner)
            {
                _inner =
                    inner
                    ?? throw new ArgumentNullException(
                        nameof(inner));
            }

            public ResourceHandle<T> Load<T>(
                string address)
                where T : UnityEngine.Object
            {
                return _inner.Load<T>(
                    address);
            }

            public async Task<ResourceHandle<T>> LoadAsync<T>(
                string address,
                CancellationToken cancellationToken = default)
                where T : UnityEngine.Object
            {
                TaskCompletionSource<bool> gate =
                    _gate;

                if (gate != null)
                {
                    await gate.Task;
                }

                cancellationToken
                    .ThrowIfCancellationRequested();

                return await _inner.LoadAsync<T>(
                    address,
                    cancellationToken);
            }

            public void Block()
            {
                if (_gate != null)
                {
                    throw new InvalidOperationException(
                        "Resource loading is already blocked.");
                }

                _gate =
                    new TaskCompletionSource<bool>();
            }

            public void Release()
            {
                TaskCompletionSource<bool> gate =
                    _gate;

                _gate = null;

                gate?.TrySetResult(true);
            }
        }
    }
}
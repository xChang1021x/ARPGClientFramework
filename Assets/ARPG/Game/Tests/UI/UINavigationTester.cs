using System;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.UI;
using ARPG.Game.UI.Confirm;
using ARPG.Game.UI.Main;
using ARPG.Game.UI.Settings;
using UnityEngine;

namespace ARPG.Game.Tests.UI
{
    /// <summary>
    /// Day18 UI Navigation测试。
    ///
    /// 验证：
    /// 1. 多UI导航顺序；
    /// 2. Back LIFO；
    /// 3. 中间Panel关闭；
    /// 4. 已存在Panel重新Open；
    /// 5. Navigation空栈行为。
    /// </summary>
    public sealed class UINavigationTester
        : MonoBehaviour
    {
        private UIService _uiService;

        private void Awake()
        {
            ServiceContainer services =
                GameLauncher.Instance
                    .GameContext
                    .Services;

            _uiService =
                services.Get<UIService>();
        }

        private void Update()
        {
            /*
             * 1:
             *
             * Open Main
             * Open Settings
             * Open Confirm
             */
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestOpenNavigationAsync();
            }

            /*
             * 2:
             * Back一次。
             */
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestBackOnce();
            }

            /*
             * 3:
             * 连续Back直到空。
             */
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestBackAll();
            }

            /*
             * 4:
             * 测试从中间关闭Settings。
             */
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestMiddleCloseAsync();
            }

            /*
             * 5:
             * 测试重新打开已有Panel。
             */
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestReopenExistingAsync();
            }

            /*
             * 6:
             * 清理三个测试UI。
             */
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                ResetPanels();
            }
        }

        /// <summary>
        /// 构建：
        ///
        /// Main
        /// ↓
        /// Settings
        /// ↓
        /// Confirm
        ///
        /// Navigation Top = Confirm。
        /// </summary>
        private async void TestOpenNavigationAsync()
        {
            try
            {
                ResetPanels();

                /*
                 * Destroy在Unity中真正销毁GameObject
                 * 会延迟到帧末。
                 *
                 * 这里给测试环境一个调度点，
                 * 避免旧测试对象与新对象同时显示。
                 */
                await Task.Yield();

                MainPanel mainPanel =
                    await _uiService
                        .OpenAsync<MainPanel>();

                SettingsPanel settingsPanel =
                    await _uiService
                        .OpenAsync<SettingsPanel>();

                ConfirmPanel confirmPanel =
                    await _uiService
                        .OpenAsync<ConfirmPanel>();

                bool passed =
                    mainPanel.IsOpen &&
                    settingsPanel.IsOpen &&
                    confirmPanel.IsOpen;

                if (passed)
                {
                    Debug.Log(
                        "[Day18] PASS: " +
                        "Main -> Settings -> Confirm opened.");
                }
                else
                {
                    Debug.LogError(
                        "[Day18] FAIL: " +
                        "Navigation setup failed.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 当前Top应该被Back关闭。
        /// </summary>
        private void TestBackOnce()
        {
            try
            {
                bool handled =
                    _uiService.Back();

                Debug.Log(
                    $"[Day18] Back handled = {handled}");

                LogPanelStates();
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 连续Back：
        ///
        /// Confirm
        /// → Settings
        /// → Main
        /// → false
        /// </summary>
        private void TestBackAll()
        {
            try
            {
                int handledCount = 0;

                while (_uiService.Back())
                {
                    handledCount++;
                }

                bool passed =
                    handledCount == 3 &&
                    !_uiService.Back();

                Debug.Log(
                    passed
                        ? $"[Day18] PASS: Back all. " +
                          $"Handled={handledCount}"
                        : "[Day18] FAIL: Back all.");

                LogPanelStates();
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
        /// Main
        /// Settings
        /// Confirm
        ///
        /// ↓ Close Settings
        ///
        /// Navigation应该等价于：
        ///
        /// Main
        /// Confirm
        ///
        /// Back:
        /// Confirm
        ///
        /// Back:
        /// Main
        /// </summary>
        private async void TestMiddleCloseAsync()
        {
            try
            {
                ResetPanels();
                await Task.Yield();

                MainPanel main =
                    await _uiService
                        .OpenAsync<MainPanel>();

                SettingsPanel settings =
                    await _uiService
                        .OpenAsync<SettingsPanel>();

                ConfirmPanel confirm =
                    await _uiService
                        .OpenAsync<ConfirmPanel>();

                _uiService.Close<SettingsPanel>();

                bool settingsClosed =
                    !settings.IsOpen;

                bool firstBack =
                    _uiService.Back();

                bool confirmClosed =
                    !confirm.IsOpen;

                bool mainStillOpen =
                    main.IsOpen;

                bool secondBack =
                    _uiService.Back();

                bool mainClosed =
                    !main.IsOpen;

                bool passed =
                    settingsClosed &&
                    firstBack &&
                    confirmClosed &&
                    mainStillOpen &&
                    secondBack &&
                    mainClosed;

                Debug.Log(
                    passed
                        ? "[Day18] PASS: Middle close navigation."
                        : "[Day18] FAIL: Middle close navigation.");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        /// <summary>
        /// 验证：
        ///
        /// Open Settings
        /// Close Settings
        /// Open Settings
        ///
        /// 第二次Open必须复用原实例。
        /// </summary>
        private async void TestReopenExistingAsync()
        {
            try
            {
                ResetPanels();
                await Task.Yield();

                SettingsPanel first =
                    await _uiService
                        .OpenAsync<SettingsPanel>();

                _uiService.Close<SettingsPanel>();

                SettingsPanel second =
                    await _uiService
                        .OpenAsync<SettingsPanel>();

                bool samePanel =
                    ReferenceEquals(
                        first,
                        second);

                bool passed =
                    samePanel &&
                    second.IsOpen;

                Debug.Log(
                    passed
                        ? "[Day18] PASS: " +
                          "Existing panel reopened without recreation."
                        : "[Day18] FAIL: " +
                          $"SamePanel={samePanel}, " +
                          $"IsOpen={second.IsOpen}");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        private void LogPanelStates()
        {
            bool hasMain =
                _uiService.TryGet<MainPanel>(
                    out MainPanel main);

            bool hasSettings =
                _uiService.TryGet<SettingsPanel>(
                    out SettingsPanel settings);

            bool hasConfirm =
                _uiService.TryGet<ConfirmPanel>(
                    out ConfirmPanel confirm);

            Debug.Log(
                "[Day18] UI States | " +
                $"Main={(hasMain && main.IsOpen)} | " +
                $"Settings={(hasSettings && settings.IsOpen)} | " +
                $"Confirm={(hasConfirm && confirm.IsOpen)}");
        }

        private void ResetPanels()
        {
            _uiService.Destroy<ConfirmPanel>();
            _uiService.Destroy<SettingsPanel>();
            _uiService.Destroy<MainPanel>();
        }
    }
}
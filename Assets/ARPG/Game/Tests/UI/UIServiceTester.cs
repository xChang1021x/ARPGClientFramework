using System;
using System.Threading.Tasks;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.UI;
using ARPG.Game.UI.Main;
using UnityEngine;

namespace ARPG.Game.Tests.UI
{
    public sealed class UIServiceTester
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
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OpenMainPanel();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _uiService.Close<MainPanel>();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _uiService.Destroy<MainPanel>();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestConcurrentOpen();
            }
        }

        private async void OpenMainPanel()
        {
            try
            {
                await _uiService
                    .OpenAsync<MainPanel>();
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        private async void TestConcurrentOpen()
        {
            try
            {
                Task<MainPanel> taskA =
                    _uiService
                        .OpenAsync<MainPanel>();

                Task<MainPanel> taskB =
                    _uiService
                        .OpenAsync<MainPanel>();

                MainPanel[] panels =
                    await Task.WhenAll(
                        taskA,
                        taskB);

                bool samePanel =
                    ReferenceEquals(
                        panels[0],
                        panels[1]);

                Debug.Log(
                    $"Same panel: {samePanel}");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }
    }
}
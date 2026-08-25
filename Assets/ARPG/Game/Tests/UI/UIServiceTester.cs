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
        }

        private async void OpenMainPanel()
        {
            await _uiService.OpenAsync<MainPanel>(
                "ARPG/UI/MainPanel",
                UILayer.Normal);
        }
    }
}
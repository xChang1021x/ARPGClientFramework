using UnityEngine;

namespace ARPG.Game.UI.Settings
{
    public sealed class SettingsPanel
        : UIPanel
    {
        protected override void OnOpen()
        {
            Debug.Log(
                "SettingsPanel opened.");
        }

        protected override void OnClose()
        {
            Debug.Log(
                "SettingsPanel closed.");
        }
    }
}
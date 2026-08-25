using UnityEngine;

namespace ARPG.Game.UI.Main
{
    public sealed class MainPanel
        : UIPanel
    {
        protected override void OnOpen()
        {
            Debug.Log(
                "MainPanel opened.");
        }

        protected override void OnClose()
        {
            Debug.Log(
                "MainPanel closed.");
        }
    }
}
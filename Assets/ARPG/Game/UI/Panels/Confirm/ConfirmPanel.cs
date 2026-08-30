using UnityEngine;

namespace ARPG.Game.UI.Confirm
{
    public sealed class ConfirmPanel
        : UIPanel
    {
        protected override void OnOpen()
        {
            Debug.Log(
                "ConfirmPanel opened.");
        }

        protected override void OnClose()
        {
            Debug.Log(
                "ConfirmPanel closed.");
        }
    }
}
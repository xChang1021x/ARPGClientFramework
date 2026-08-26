using UnityEngine;

namespace ARPG.Game.UI
{
    public abstract class UIPanel
        : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        internal void InitializeClosed()
        {
            IsOpen = false;

            gameObject.SetActive(false);
        }

        internal void Open()
        {
            if (IsOpen)
            {
                return;
            }

            gameObject.SetActive(true);

            IsOpen = true;

            OnOpen();
        }

        internal void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            OnClose();

            IsOpen = false;

            gameObject.SetActive(false);
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }
    }
}
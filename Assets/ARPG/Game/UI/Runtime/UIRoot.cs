using System;
using UnityEngine;

namespace ARPG.Game.UI
{
    public sealed class UIRoot : MonoBehaviour
    {
        [SerializeField]
        private Transform _backgroundRoot;

        [SerializeField]
        private Transform _normalRoot;

        [SerializeField]
        private Transform _popupRoot;

        [SerializeField]
        private Transform _overlayRoot;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public Transform GetLayerRoot(
            UILayer layer)
        {
            return layer switch
            {
                UILayer.Background =>
                    _backgroundRoot,

                UILayer.Normal =>
                    _normalRoot,

                UILayer.Popup =>
                    _popupRoot,

                UILayer.Overlay =>
                    _overlayRoot,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(layer),
                    layer,
                    null)
            };
        }
    }
}
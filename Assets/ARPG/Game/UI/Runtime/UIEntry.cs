using UnityEngine;
using ARPG.Game.Resource;

namespace ARPG.Game.UI
{
    internal sealed class UIEntry
    {
        public UIEntry(
            UIPanel panel,
            ResourceHandle<GameObject> resourceHandle,
            UILayer layer)
        {
            Panel = panel;
            ResourceHandle = resourceHandle;
            Layer = layer;
        }

        public UIPanel Panel { get; }

        public ResourceHandle<GameObject>
            ResourceHandle
        { get; }

        public UILayer Layer { get; }
    }
}
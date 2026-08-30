using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.UI
{
    /// <summary>
    /// 一个已经创建完成的UI运行时记录。
    ///
    /// 同时拥有：
    /// 1. Panel实例；
    /// 2. Prefab ResourceHandle；
    /// 3. UI静态运行时配置。
    /// </summary>
    internal sealed class UIEntry
    {
        public UIEntry(
            UIPanel panel,
            ResourceHandle<GameObject> resourceHandle,
            UIConfig config)
        {
            Panel = panel;
            ResourceHandle = resourceHandle;
            Config = config;
        }

        public UIPanel Panel { get; }

        public ResourceHandle<GameObject>
            ResourceHandle
        {
            get;
        }

        public UIConfig Config { get; }
    }
}
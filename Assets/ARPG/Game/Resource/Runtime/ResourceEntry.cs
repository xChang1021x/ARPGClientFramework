using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// ResourceService内部缓存条目。
    /// </summary>
    internal sealed class ResourceEntry
    {
        public ResourceEntry(
            UnityEngine.Object asset)
        {
            Asset = asset;
        }

        public UnityEngine.Object Asset { get; }

        public int ReferenceCount { get; set; }
    }
}
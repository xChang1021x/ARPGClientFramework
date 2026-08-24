using System;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 唯一标识一个资源请求。
    /// 同一路径不同类型视为不同资源。
    /// </summary>
    internal readonly struct ResourceKey
        : IEquatable<ResourceKey>
    {
        public ResourceKey(
            Type resourceType,
            string path)
        {
            ResourceType = resourceType
                ?? throw new ArgumentNullException(
                    nameof(resourceType));

            Path = path
                ?? throw new ArgumentNullException(
                    nameof(path));
        }

        public Type ResourceType { get; }

        public string Path { get; }

        public bool Equals(ResourceKey other)
        {
            return ResourceType == other.ResourceType &&
                   string.Equals(
                       Path,
                       other.Path,
                       StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceKey other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash =
                    hash * 31 +
                    ResourceType.GetHashCode();

                hash =
                    hash * 31 +
                    StringComparer.Ordinal
                        .GetHashCode(Path);

                return hash;
            }
        }

        public override string ToString()
        {
            return
                $"{ResourceType.Name}:{Path}";
        }
    }
}
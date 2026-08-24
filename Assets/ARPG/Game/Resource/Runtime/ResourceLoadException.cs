using System;

namespace ARPG.Game.Resource
{
    public sealed class ResourceLoadException
        : Exception
    {
        public ResourceLoadException(
            Type resourceType,
            string path)
            : base(
                $"Failed to load resource " +
                $"'{path}' as " +
                $"'{resourceType.Name}'.")
        {
            ResourceType = resourceType;
            Path = path;
        }

        public Type ResourceType { get; }

        public string Path { get; }
    }
}
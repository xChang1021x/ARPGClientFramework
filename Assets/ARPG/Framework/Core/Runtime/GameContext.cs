using System;

namespace ARPG.Framework.Core
{
    /// <summary>
    /// 游戏运行时上下文。
    /// 持有运行时服务容器。
    /// </summary>
    public sealed class GameContext : IDisposable
    {
        public ServiceContainer Services { get; }

        public GameContext()
        {
            Services =
                new ServiceContainer();
        }

        public void Initialize()
        {
            Services.Initialize();
        }

        public void Dispose()
        {
            Services.Dispose();
        }
    }
}
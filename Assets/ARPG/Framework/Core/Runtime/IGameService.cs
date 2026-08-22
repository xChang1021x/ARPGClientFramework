namespace ARPG.Framework.Core
{
    /// <summary>
    /// 客户端基础服务统一生命周期接口。
    /// </summary>
    public interface IGameService
        : IInitializable, IShutdownable
    {
    }
}
namespace ARPG.Framework.Core
{
    /// <summary>
    /// 客户端基础服务统一生命周期接口。
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// 初始化服务。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 关闭并释放服务。
        /// </summary>
        void Shutdown();
    }
}
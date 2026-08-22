namespace ARPG.Framework.Core
{
    /// <summary>
    /// 表示对象需要显式关闭。
    /// </summary>
    public interface IShutdownable
    {
        void Shutdown();
    }
}
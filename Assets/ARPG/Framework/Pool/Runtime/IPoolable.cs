namespace ARPG.Framework.Pool
{
    /// <summary>
    /// 可池化对象生命周期接口。
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 从池中获取时调用。
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 回收到池中时调用。
        /// </summary>
        void OnDespawn();
    }
}
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 游戏资源访问接口。
    ///
    /// 所有资源加载均返回ResourceHandle，
    /// 由Handle显式表达资源所有权。
    /// 调用方使用完成后必须Dispose。
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 同步加载资源。
        ///
        /// 正式游戏流程应优先使用LoadAsync，
        /// 此方法仅适用于明确允许同步阻塞的场景。
        /// </summary>
        ResourceHandle<T> Load<T>(
            string address)
            where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载资源。
        ///
        /// CancellationToken取消的是当前调用方对结果的消费，
        /// 不保证真正取消共享的底层资源加载操作。
        /// </summary>
        Task<ResourceHandle<T>> LoadAsync<T>(
            string address,
            CancellationToken cancellationToken = default)
            where T : UnityEngine.Object;
    }
}
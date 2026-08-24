using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 游戏资源访问接口。
    /// 调用方不应依赖具体资源加载实现。
    /// </summary>
    public interface IResourceService
    {
        T Load<T>(string path)
            where T : Object;

        bool TryLoad<T>(
            string path,
            out T asset)
            where T : Object;
    }
}
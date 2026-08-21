using ARPG.Framework.Config;

namespace ARPG.Game.Config
{
    /// <summary>
    /// Unity配置资产统一注册接口。
    /// 将编辑器资产转换为运行时配置并注册到ConfigService。
    /// </summary>
    public interface IConfigAsset
    {
        void Register(ConfigService configService);
    }
}
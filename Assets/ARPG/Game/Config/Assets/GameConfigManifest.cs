using System;
using ARPG.Framework.Config;
using UnityEngine;

namespace ARPG.Game.Config
{
    /// <summary>
    /// 游戏配置资产清单。
    /// 集中管理启动阶段需要加载的配置资产。
    /// </summary>
    [CreateAssetMenu(
        fileName = "GameConfigManifest",
        menuName = "ARPG/Config/Game Config Manifest")]
    public sealed class GameConfigManifest : ScriptableObject
    {
        [SerializeField]
        private ScriptableObject[] _configAssets =
            Array.Empty<ScriptableObject>();

        /// <summary>
        /// 将所有配置资产转换并注册到ConfigService。
        /// </summary>
        public void RegisterAll(
            ConfigService configService)
        {
            if (configService == null)
            {
                throw new ArgumentNullException(
                    nameof(configService));
            }

            for (int index = 0;
                 index < _configAssets.Length;
                 index++)
            {
                ScriptableObject asset =
                    _configAssets[index];

                if (asset == null)
                {
                    throw new InvalidOperationException(
                        $"Config asset at index {index} is null.");
                }

                if (!(asset is IConfigAsset configAsset))
                {
                    throw new InvalidOperationException(
                        $"Config asset '{asset.name}' " +
                        $"does not implement {nameof(IConfigAsset)}.");
                }

                configAsset.Register(
                    configService);
            }
        }
    }
}
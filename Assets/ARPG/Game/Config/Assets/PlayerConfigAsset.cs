using System;
using ARPG.Framework.Config;
using UnityEngine;

namespace ARPG.Game.Config
{
    /// <summary>
    /// 玩家配置的Unity编辑器资产。
    /// 负责Inspector数据编辑以及向运行时配置转换。
    /// </summary>
    [CreateAssetMenu(
        fileName = "PlayerConfig",
        menuName = "ARPG/Config/Player Config")]
    public sealed class PlayerConfigAsset
        : ScriptableObject, IConfigAsset
    {
        [Header("Basic Attributes")]

        [SerializeField]
        [Min(1)]
        private int _maxHealth = 1000;

        [SerializeField]
        [Min(0)]
        private int _attack = 100;

        [SerializeField]
        [Min(0.01f)]
        private float _moveSpeed = 5f;

        /// <summary>
        /// 将Unity资产转换为纯C#运行时配置。
        /// </summary>
        public PlayerConfig ToRuntimeConfig()
        {
            return new PlayerConfig(
                maxHealth: _maxHealth,
                attack: _attack,
                moveSpeed: _moveSpeed);
        }

        public void Register(
            ConfigService configService)
        {
            if (configService == null)
            {
                throw new ArgumentNullException(
                    nameof(configService));
            }

            configService.Register(
                ToRuntimeConfig());
        }
    }
}
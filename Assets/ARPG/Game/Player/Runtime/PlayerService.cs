using System;
using ARPG.Framework.Config;
using ARPG.Framework.Core;
using ARPG.Game.Config;

namespace ARPG.Game.Player
{
    /// <summary>
    /// 玩家领域服务。
    /// 当前负责读取并暴露玩家基础配置。
    /// </summary>
    public sealed class PlayerService
        : IPlayerService, IGameService
    {
        private readonly ConfigService _configService;

        private PlayerConfig _playerConfig;

        public int MaxHealth =>
            GetConfig().MaxHealth;

        public int Attack =>
            GetConfig().Attack;

        public float MoveSpeed =>
            GetConfig().MoveSpeed;

        public PlayerService(
            ConfigService configService)
        {
            _configService =
                configService
                ?? throw new ArgumentNullException(
                    nameof(configService));
        }

        public void Initialize()
        {
            _playerConfig =
                _configService
                    .Get<PlayerConfig>();
        }

        public void Shutdown()
        {
            _playerConfig = null;
        }

        private PlayerConfig GetConfig()
        {
            if (_playerConfig == null)
            {
                throw new InvalidOperationException(
                    "PlayerService has not been initialized.");
            }

            return _playerConfig;
        }
    }
}
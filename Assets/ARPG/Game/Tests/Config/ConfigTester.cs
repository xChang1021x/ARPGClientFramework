using ARPG.Game.Bootstrap;
using ARPG.Game.Config;
using UnityEngine;

namespace ARPG.Game.Tests.Config
{
    public sealed class ConfigTester : MonoBehaviour
    {
        private void Start()
        {
            PlayerConfig playerConfig =
                GameLauncher.Instance
                    .GameContext
                    .ConfigService
                    .Get<PlayerConfig>();

            Debug.Log(
                $"MaxHealth={playerConfig.MaxHealth}, " +
                $"Attack={playerConfig.Attack}, " +
                $"MoveSpeed={playerConfig.MoveSpeed}");
        }
    }
}
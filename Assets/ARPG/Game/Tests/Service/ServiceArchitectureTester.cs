using ARPG.Framework.Core;
using ARPG.Game.Player;
using UnityEngine;

namespace ARPG.Game.Tests.Service
{
    public sealed class ServiceArchitectureTester
        : MonoBehaviour
    {
        private void Start()
        {
            ServiceContainer services =
                Bootstrap.GameLauncher.Instance
                    .GameContext
                    .Services;

            IPlayerService playerService =
                services.Get<IPlayerService>();

            Debug.Log(
                $"Player Service Ready: " +
                $"HP={playerService.MaxHealth}, " +
                $"Attack={playerService.Attack}, " +
                $"MoveSpeed={playerService.MoveSpeed}");
        }
    }
}
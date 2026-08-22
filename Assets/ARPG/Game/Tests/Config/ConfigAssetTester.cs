// using ARPG.Game.Bootstrap;
// using ARPG.Game.Config;
// using UnityEngine;

// namespace ARPG.Game.Tests.Config
// {
//     public sealed class ConfigAssetTester
//         : MonoBehaviour
//     {
//         private void Start()
//         {
//             PlayerConfig playerConfig =
//                 GameLauncher.Instance
//                     .GameContext
//                     .ConfigService
//                     .Get<PlayerConfig>();

//             GameLauncher.Instance
//                 .GameContext
//                 .LogService
//                 .Info(
//                     "ConfigTest",
//                     $"PlayerConfig loaded: " +
//                     $"HP={playerConfig.MaxHealth}, " +
//                     $"Attack={playerConfig.Attack}, " +
//                     $"MoveSpeed={playerConfig.MoveSpeed}");
//         }
//     }
// }
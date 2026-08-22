// using System;
// using ARPG.Framework.Event;
// using ARPG.Game.Bootstrap;
// using ARPG.Game.Events;
// using UnityEngine;

// namespace ARPG.Game.Tests
// {
//     public sealed class BattleUIListener : MonoBehaviour
//     {
//         private EventBus _eventBus = null!;
//         private IDisposable _subscription;

//         private void OnEnable()
//         {
//             _eventBus =
//                 GameLauncher.Instance.GameContext.EventBus;

//             _subscription =
//                 _eventBus.Subscribe<CharacterDiedEvent>(
//                     OnCharacterDied);

//             Debug.Log("[Listener] Subscribed.");
//         }

//         private void OnDisable()
//         {
//             _subscription?.Dispose();
//             _subscription = null;

//             Debug.Log("[Listener] Unsubscribed.");
//         }

//         private static void OnCharacterDied(
//             CharacterDiedEvent eventData)
//         {
//             if (!eventData.IsPlayer)
//             {
//                 return;
//             }

//             Debug.Log(
//                 $"显示死亡界面，角色ID：{eventData.CharacterId}");
//         }
//     }
// }
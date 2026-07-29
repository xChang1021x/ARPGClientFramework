using System;
using ARPG.Framework.Event;
using ARPG.Game.Events;
using UnityEngine;

namespace ARPG.Game.Tests
{
    public sealed class BattleResultListener : MonoBehaviour
    {
        private EventBus _eventBus = null!;
        private IDisposable _subscription;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnEnable()
        {
            if (_eventBus is null)
            {
                return;
            }

            _subscription =
                _eventBus.Subscribe<CharacterDiedEvent>(
                    OnCharacterDied);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private static void OnCharacterDied(
            CharacterDiedEvent eventData)
        {
            if (!eventData.IsPlayer)
            {
                return;
            }

            Debug.Log(
                $"战斗结果：玩家死亡，角色ID：{eventData.CharacterId}");
        }
    }
}
using ARPG.Framework.Event;
using ARPG.Game.Bootstrap;
using ARPG.Game.Events;
using UnityEngine;

namespace ARPG.Game.Tests
{
    /// <summary>
    /// EventBus测试发布者。
    /// 按K键模拟角色死亡事件。
    /// </summary>
    public sealed class CharacterDeathPublisher : MonoBehaviour
    {
        private EventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameLauncher.Instance.GameContext.EventBus;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.K))
            {
                return;
            }

            CharacterDiedEvent eventData =
                new CharacterDiedEvent(
                    characterId: 1001,
                    isPlayer: true);

            _eventBus.Publish(eventData);

            Debug.Log("[Publisher] CharacterDiedEvent published.");
        }
    }
}
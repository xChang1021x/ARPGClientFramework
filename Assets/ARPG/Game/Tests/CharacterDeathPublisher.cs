using ARPG.Framework.Event;
using ARPG.Game.Events;
using UnityEngine;

namespace ARPG.Game.Tests
{
    public sealed class CharacterDeathPublisher : MonoBehaviour
    {
        private EventBus _eventBus = null!;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.K))
            {
                return;
            }

            var eventData = new CharacterDiedEvent(
                characterId: 1001,
                isPlayer: true);

            _eventBus.Publish(eventData);
        }
    }
}
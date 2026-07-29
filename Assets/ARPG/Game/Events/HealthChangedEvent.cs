using ARPG.Framework.Event;

namespace ARPG.Game.Events
{
    /// <summary>
    /// 生命值变化后的通知事件。
    /// </summary>
    public readonly struct HealthChangedEvent : IEvent
    {
        public int CharacterId { get; }

        public bool IsPlayer { get; }

        public int HealthValue { get; }

        public HealthChangedEvent(
            int characterId,
            int healthValue,
            bool isPlayer)
        {
            CharacterId = characterId;
            HealthValue = healthValue;
            IsPlayer = isPlayer;
        }
    }
}
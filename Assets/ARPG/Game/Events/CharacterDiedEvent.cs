using ARPG.Framework.Event;

namespace ARPG.Game.Events
{
    /// <summary>
    /// 角色死亡后的通知事件。
    /// </summary>
    public readonly struct CharacterDiedEvent : IEvent
    {
        public int CharacterId { get; }

        public bool IsPlayer { get; }

        public CharacterDiedEvent(
            int characterId,
            bool isPlayer)
        {
            CharacterId = characterId;
            IsPlayer = isPlayer;
        }
    }
}
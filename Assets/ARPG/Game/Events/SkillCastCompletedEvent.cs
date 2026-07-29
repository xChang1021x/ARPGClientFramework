using ARPG.Framework.Event;

namespace ARPG.Game.Events
{
    /// <summary>
    /// 技能释放完成的通知事件。
    /// </summary>
    public readonly struct SkillCastCompletedEvent : IEvent
    {
        public int CharacterId { get; }

        public int SkillId { get; }

        public SkillCastCompletedEvent(
            int characterId,
            int skillId)
        {
            CharacterId = characterId;
            SkillId = skillId;
        }
    }
}
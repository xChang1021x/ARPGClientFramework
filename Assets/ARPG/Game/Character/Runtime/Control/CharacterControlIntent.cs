using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.Control
{
    /// <summary>
    /// 单帧Character控制意图。
    ///
    /// 描述控制者希望角色做什么，
    /// 不描述输入设备来源。
    /// </summary>
    public readonly struct CharacterControlIntent
    {
        public CharacterControlIntent(
            CharacterMovementIntent movement,
            bool attackPressed)
        {
            Movement = movement;
            AttackPressed = attackPressed;
        }

        public CharacterMovementIntent Movement { get; }

        public bool AttackPressed { get; }
    }
}
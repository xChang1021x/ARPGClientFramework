using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character
{
    /// <summary>
    /// 一个Character实例自己的运行时上下文。
    ///
    /// 后续会逐渐加入：
    /// Attribute
    /// StateMachine
    /// Skill
    /// Buff
    /// Animation
    /// 等角色级模块。
    /// </summary>
    public sealed class CharacterContext
    {
        public CharacterContext(
            CharacterConfig config,
            CharacterMotor motor)
        {
            Config = config;

            Motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));
        }

        public CharacterConfig Config { get; }

        public CharacterMotor Motor { get; }
    }
}
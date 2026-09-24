using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    /// <summary>
    /// Character死亡状态。
    ///
    /// 第一版：
    /// - 禁止水平移动；
    /// - 保留Motor Tick以维持重力/贴地；
    /// - 不允许自动离开。
    /// </summary>
    public sealed class CharacterDeadState
        : ICharacterState
    {
        private static readonly CharacterMovementIntent
            NoMovement =
                new CharacterMovementIntent(
                    UnityEngine.Vector3.zero);

        private readonly CharacterMotor _motor;

        public CharacterDeadState(
            CharacterMotor motor)
        {
            _motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));
        }

        public void Enter()
        {
        }

        public void Tick(
            CharacterMovementIntent movementIntent,
            float deltaTime)
        {
            _motor.Tick(
                NoMovement,
                deltaTime);
        }

        public void Exit()
        {
        }
    }
}
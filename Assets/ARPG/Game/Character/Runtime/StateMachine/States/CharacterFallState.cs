using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    /// <summary>
    /// Character离地/下落状态。
    ///
    /// 第一版允许空中保持水平输入。
    /// </summary>
    public sealed class CharacterFallState
        : ICharacterState
    {
        private readonly CharacterStateMachine
            _stateMachine;

        private readonly CharacterMotor
            _motor;

        public CharacterFallState(
            CharacterStateMachine stateMachine,
            CharacterMotor motor)
        {
            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

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
                movementIntent,
                deltaTime);

            if (!_motor.IsGrounded)
            {
                return;
            }

            if (movementIntent.HasMovement)
            {
                _stateMachine
                    .ChangeState<CharacterMoveState>();

                return;
            }

            _stateMachine
                .ChangeState<CharacterIdleState>();
        }

        public void Exit()
        {
        }
    }
}
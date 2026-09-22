using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    /// <summary>
    /// Character普通移动状态。
    /// </summary>
    public sealed class CharacterMoveState
        : ICharacterState
    {
        private readonly CharacterStateMachine
            _stateMachine;

        private readonly CharacterMotor
            _motor;

        public CharacterMoveState(
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
                _stateMachine
                    .ChangeState<CharacterFallState>();

                return;
            }

            if (!movementIntent.HasMovement)
            {
                _stateMachine
                    .ChangeState<CharacterIdleState>();
            }
        }

        public void Exit()
        {
        }
    }
}
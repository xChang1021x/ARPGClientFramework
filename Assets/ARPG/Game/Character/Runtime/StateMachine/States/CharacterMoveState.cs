using System;
using ARPG.Game.Character.Control;
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
            CharacterControlIntent intent,
            float deltaTime)
        {
            _motor.Tick(
                intent.Movement,
                deltaTime);

            if (!_motor.IsGrounded)
            {
                _stateMachine
                    .ChangeState<CharacterFallState>();

                return;
            }

            if (intent.AttackPressed)
            {
                _stateMachine.ChangeState<CharacterAttackState>();
                return;
            }

            if (!intent.Movement.HasMovement)
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
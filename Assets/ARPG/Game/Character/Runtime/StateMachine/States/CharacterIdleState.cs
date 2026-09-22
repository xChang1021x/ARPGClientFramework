using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    /// <summary>
    /// Character站立状态。
    /// </summary>
    public sealed class CharacterIdleState
        : ICharacterState
    {
        private static readonly CharacterMovementIntent
            NoMovement =
                new CharacterMovementIntent(
                    UnityEngine.Vector2.zero);

        private readonly CharacterStateMachine
            _stateMachine;

        private readonly CharacterMotor
            _motor;

        public CharacterIdleState(
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
            /*
             * 即使Idle也必须Tick Motor，
             * 因为Gravity不能停止。
             */
            _motor.Tick(
                NoMovement,
                deltaTime);

            /*
             * 优先处理离地。
             */
            if (!_motor.IsGrounded)
            {
                _stateMachine
                    .ChangeState<CharacterFallState>();

                return;
            }

            if (movementIntent.HasMovement)
            {
                _stateMachine
                    .ChangeState<CharacterMoveState>();
            }
        }

        public void Exit()
        {
        }
    }
}
using System;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    /// <summary>
    /// Character普通受击硬直状态。
    /// </summary>
    public sealed class CharacterHitState
        : ICharacterState
    {
        private static readonly CharacterMovementIntent
            NoMovement =
                new CharacterMovementIntent(
                    UnityEngine.Vector3.zero);

        private readonly CharacterStateMachine
            _stateMachine;

        private readonly CharacterMotor
            _motor;

        private readonly float
            _recoveryDuration;

        private float _elapsedTime;

        public CharacterHitState(
            CharacterStateMachine stateMachine,
            CharacterMotor motor,
            float recoveryDuration)
        {
            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));

            if (recoveryDuration < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(recoveryDuration));
            }

            _recoveryDuration =
                recoveryDuration;
        }

        public void Enter()
        {
            _elapsedTime = 0f;
        }

        public void Tick(
            CharacterMovementIntent movementIntent,
            float deltaTime)
        {
            /*
             * Hit期间禁止普通水平移动，
             * 但继续Gravity。
             */
            _motor.Tick(
                NoMovement,
                deltaTime);

            _elapsedTime +=
                deltaTime;

            if (_elapsedTime <
                _recoveryDuration)
            {
                return;
            }

            /*
             * 恢复时先判断是否仍在空中。
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
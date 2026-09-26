using System;
using ARPG.Game.Character.Control;
using ARPG.Game.Character.Movement;

namespace ARPG.Game.Character.StateMachine.States
{
    public sealed class CharacterAttackState
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

        private readonly float _duration;

        private float _elapsedTime;

        public CharacterAttackState(
            CharacterStateMachine stateMachine,
            CharacterMotor motor,
            float duration)
        {
            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));

            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(duration));
            }

            _duration =
                duration;
        }

        public void Enter()
        {
            _elapsedTime = 0f;
        }

        public void Tick(
            CharacterControlIntent intent,
            float deltaTime)
        {
            /*
             * 第一版普通攻击锁定水平移动，
             * 但依然保留重力。
             */
            _motor.Tick(
                NoMovement,
                deltaTime);

            _elapsedTime +=
                deltaTime;

            if (_elapsedTime < _duration)
            {
                return;
            }

            if (!_motor.IsGrounded)
            {
                _stateMachine
                    .ChangeState<CharacterFallState>();

                return;
            }

            if (intent.Movement.HasMovement)
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
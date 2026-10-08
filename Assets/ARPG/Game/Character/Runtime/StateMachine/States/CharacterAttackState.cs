using System;
using ARPG.Game.Character.Control;
using ARPG.Game.Character.Movement;
using ARPG.Game.Combat.Attack;

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

        private readonly CharacterAttackExecutor
    _attackExecutor;

        private readonly float _hitTime;

        private bool _hasExecutedHit;

        public CharacterAttackState(
            CharacterStateMachine stateMachine,
            CharacterMotor motor,
            CharacterAttackExecutor attackExecutor,
            float duration,
            float hitTime)
        {


            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));

            _attackExecutor =
                attackExecutor
                ?? throw new ArgumentNullException(
                    nameof(attackExecutor));

            if (duration <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(duration));
            }

            _duration =
                duration;

            if (hitTime < 0f ||
                hitTime > duration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hitTime));
            }

            _hitTime = hitTime;
        }

        public void Enter()
        {
            _elapsedTime = 0f;
            _hasExecutedHit = false;
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

            if (!_hasExecutedHit &&
                _elapsedTime >= _hitTime)
            {
                _hasExecutedHit = true;

                _attackExecutor.Execute();
            }

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
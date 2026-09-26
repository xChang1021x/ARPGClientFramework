using System;
using System.Collections.Generic;
using ARPG.Game.Character.Control;
using ARPG.Game.Character.Movement;
using ARPG.Game.Character.StateMachine.States;

namespace ARPG.Game.Character.StateMachine
{
    /// <summary>
    /// Character有限状态机。
    ///
    /// 负责：
    /// - 状态实例注册；
    /// - 当前状态管理；
    /// - 状态切换生命周期；
    /// - 将Character Intent转发给当前状态。
    ///
    /// 不负责：
    /// - 读取输入；
    /// - Movement物理实现；
    /// - Animator播放。
    /// </summary>
    public sealed class CharacterStateMachine
    {
        private readonly Dictionary<Type, ICharacterState>
            _states = new();

        private ICharacterState _currentState;

        public CharacterStateMachine(
            CharacterMotor motor,
            float hitRecoveryDuration,
            float attackDuration)
        {
            if (motor == null)
            {
                throw new ArgumentNullException(
                    nameof(motor));
            }

            Register(
                new CharacterIdleState(
                    this,
                    motor));

            Register(
                new CharacterMoveState(
                    this,
                    motor));

            Register(
                new CharacterFallState(
                    this,
                    motor));

            Register(
                new CharacterHitState(
                    this,
                    motor,
                    hitRecoveryDuration));

            Register(
                new CharacterDeadState(
                    motor));

            Register(
                new CharacterAttackState(
                    this,
                    motor,
                    attackDuration));
        }

        public ICharacterState CurrentState =>
            _currentState;

        public Type CurrentStateType =>
            _currentState?.GetType();

        public void Start<TState>()
            where TState : class, ICharacterState
        {
            if (_currentState != null)
            {
                throw new InvalidOperationException(
                    "Character state machine has already started.");
            }

            ChangeState<TState>();
        }

        public void Tick(
            CharacterControlIntent intent,
            float deltaTime)
        {
            if (_currentState == null)
            {
                throw new InvalidOperationException(
                    "Character state machine has not been started.");
            }

            _currentState.Tick(
                intent,
                deltaTime);
        }

        public void ChangeState<TState>()
            where TState : class, ICharacterState
        {
            Type targetType =
                typeof(TState);

            if (!_states.TryGetValue(
                    targetType,
                    out ICharacterState targetState))
            {
                throw new InvalidOperationException(
                    $"Character state '{targetType.Name}' " +
                    "has not been registered.");
            }

            if (ReferenceEquals(
                    _currentState,
                    targetState))
            {
                return;
            }

            _currentState?.Exit();

            _currentState =
                targetState;

            _currentState.Enter();
        }

        private void Register<TState>(
            TState state)
            where TState : class, ICharacterState
        {
            if (state == null)
            {
                throw new ArgumentNullException(
                    nameof(state));
            }

            Type stateType =
                typeof(TState);

            if (!_states.TryAdd(
                    stateType,
                    state))
            {
                throw new InvalidOperationException(
                    $"Character state '{stateType.Name}' " +
                    "has already been registered.");
            }
        }

        public void RestartState<TState>()
            where TState : class, ICharacterState
        {
            Type targetType =
                typeof(TState);

            if (!_states.TryGetValue(
                    targetType,
                    out ICharacterState targetState))
            {
                throw new InvalidOperationException(
                    $"Character state '{targetType.Name}' " +
                    "has not been registered.");
            }

            if (!ReferenceEquals(
                    _currentState,
                    targetState))
            {
                ChangeState<TState>();
                return;
            }

            _currentState.Exit();
            _currentState.Enter();
        }
    }
}
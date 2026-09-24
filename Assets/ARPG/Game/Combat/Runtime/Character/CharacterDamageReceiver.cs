using System;
using ARPG.Game.Character.Attribute;
using ARPG.Game.Character.StateMachine;
using ARPG.Game.Character.StateMachine.States;
using ARPG.Game.Combat.Damage;

namespace ARPG.Game.Combat.Character
{
    /// <summary>
    /// Character战斗受伤协调器。
    ///
    /// 负责：
    /// DamageRequest
    /// → DamageResolver
    /// → 根据DamageResult驱动角色战斗反应。
    ///
    /// 不负责伤害计算本身。
    /// </summary>
    public sealed class CharacterDamageReceiver
    {
        private readonly CharacterHealth _health;
        private readonly CharacterStateMachine _stateMachine;
        private readonly DamageResolver _damageResolver;

        public CharacterDamageReceiver(
            CharacterHealth health,
            CharacterStateMachine stateMachine,
            DamageResolver damageResolver)
        {
            _health =
                health
                ?? throw new ArgumentNullException(
                    nameof(health));

            _stateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));

            _damageResolver =
                damageResolver
                ?? throw new ArgumentNullException(
                    nameof(damageResolver));
        }

        public DamageResult Receive(
            DamageRequest request)
        {
            DamageResult result =
                _damageResolver.Resolve(
                    _health,
                    request);

            if (!result.Applied)
            {
                return result;
            }

            if (result.Killed)
            {
                _stateMachine
                    .ChangeState<CharacterDeadState>();

                return result;
            }

            _stateMachine
                .RestartState<CharacterHitState>();

            return result;
        }
    }
}
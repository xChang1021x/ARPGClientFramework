using System;
using ARPG.Game.Character.Attribute;

namespace ARPG.Game.Combat.Damage
{
    /// <summary>
    /// Damage处理协调器。
    ///
    /// 负责：
    /// DamageRequest
    /// →
    /// CharacterHealth
    /// →
    /// DamageResult。
    ///
    /// 当前不负责：
    /// - Defense
    /// - Critical
    /// - Buff
    /// - Animation
    /// - VFX
    /// - Death State
    /// </summary>
    public sealed class DamageResolver
    {
        public DamageResult Resolve(
            CharacterHealth targetHealth,
            DamageRequest request)
        {
            if (targetHealth == null)
            {
                throw new ArgumentNullException(
                    nameof(targetHealth));
            }

            int healthBefore =
                targetHealth.CurrentHealth;

            bool wasAlive =
                targetHealth.IsAlive;

            targetHealth.TakeDamage(
                request.Damage);

            int healthAfter =
                targetHealth.CurrentHealth;

            int appliedDamage =
                healthBefore -
                healthAfter;

            bool killed =
                wasAlive &&
                !targetHealth.IsAlive &&
                appliedDamage > 0;

            return new DamageResult(
                request.Damage,
                appliedDamage,
                healthAfter,
                killed);
        }
    }
}
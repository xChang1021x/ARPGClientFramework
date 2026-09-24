using System;

namespace ARPG.Game.Combat.Damage
{
    /// <summary>
    /// 一次伤害处理请求。
    ///
    /// 当前只描述基础伤害值。
    /// 后续可以扩展：
    /// - Attacker
    /// - DamageType
    /// - SkillId
    /// - Critical
    /// 等战斗上下文。
    /// </summary>
    public readonly struct DamageRequest
    {
        public DamageRequest(
            int damage)
        {
            if (damage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damage));
            }

            Damage = damage;
        }

        public int Damage { get; }
    }
}
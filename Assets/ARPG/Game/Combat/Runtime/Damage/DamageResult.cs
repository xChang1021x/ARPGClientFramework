namespace ARPG.Game.Combat.Damage
{
    /// <summary>
    /// 一次DamageRequest处理完成后的结果。
    /// </summary>
    public readonly struct DamageResult
    {
        public DamageResult(
            int requestedDamage,
            int appliedDamage,
            int remainingHealth,
            bool killed)
        {
            RequestedDamage =
                requestedDamage;

            AppliedDamage =
                appliedDamage;

            RemainingHealth =
                remainingHealth;

            Killed =
                killed;
        }

        /// <summary>
        /// 请求造成的伤害值。
        /// </summary>
        public int RequestedDamage { get; }

        /// <summary>
        /// 实际从目标生命值中扣除的数值。
        /// </summary>
        public int AppliedDamage { get; }

        /// <summary>
        /// Damage处理后的剩余生命。
        /// </summary>
        public int RemainingHealth { get; }

        /// <summary>
        /// 目标是否由“本次伤害”从存活变为死亡。
        /// </summary>
        public bool Killed { get; }

        public bool Applied =>
            AppliedDamage > 0;
    }
}
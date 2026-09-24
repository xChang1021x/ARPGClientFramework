using System;

namespace ARPG.Game.Character.Attribute
{
    /// <summary>
    /// Character运行时生命状态。
    ///
    /// 只负责生命值本身的规则，
    /// 不负责伤害公式、动画、UI或死亡表现。
    /// </summary>
    public sealed class CharacterHealth
    {
        public CharacterHealth(
            int maxHealth)
        {
            if (maxHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHealth));
            }

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public int MaxHealth { get; }

        public int CurrentHealth { get; private set; }

        public bool IsAlive =>
            CurrentHealth > 0;

        public bool TakeDamage(
            int damage)
        {
            if (damage <= 0 ||
                !IsAlive)
            {
                return false;
            }

            int previousHealth =
                CurrentHealth;

            CurrentHealth =
                Math.Max(
                    0,
                    CurrentHealth - damage);

            return CurrentHealth !=
                   previousHealth;
        }

        public bool Heal(
            int amount)
        {
            if (amount <= 0 ||
                !IsAlive)
            {
                return false;
            }

            int previousHealth =
                CurrentHealth;

            CurrentHealth =
                Math.Min(
                    MaxHealth,
                    CurrentHealth + amount);

            return CurrentHealth !=
                   previousHealth;
        }
    }
}
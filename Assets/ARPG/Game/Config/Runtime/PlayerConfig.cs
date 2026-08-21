using ARPG.Framework.Config;

namespace ARPG.Game.Config
{
    /// <summary>
    /// 玩家基础属性配置。
    /// </summary>
    public sealed class PlayerConfig : IConfig
    {
        public int MaxHealth { get; }

        public int Attack { get; }

        public float MoveSpeed { get; }

        public PlayerConfig(
            int maxHealth,
            int attack,
            float moveSpeed)
        {
            if (maxHealth <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(maxHealth));
            }

            if (attack < 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(attack));
            }

            if (moveSpeed <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(moveSpeed));
            }

            MaxHealth = maxHealth;
            Attack = attack;
            MoveSpeed = moveSpeed;
        }
    }
}
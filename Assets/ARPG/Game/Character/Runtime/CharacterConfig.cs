using System;

namespace ARPG.Game.Character
{
    /// <summary>
    /// 一个角色类型的静态运行时配置。
    /// </summary>
    public readonly struct CharacterConfig
    {
        public CharacterConfig(
            string address,
            string displayName,
            float moveSpeed,
            float gravity,
            float rotationSpeed)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException(
                    "Character address cannot be empty.",
                    nameof(address));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Character display name cannot be empty.",
                    nameof(displayName));
            }

            if (moveSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveSpeed));
            }

            if (gravity >= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gravity),
                    "Gravity must be negative.");
            }

            if (rotationSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rotationSpeed));
            }

            Address = address;
            DisplayName = displayName;
            MoveSpeed = moveSpeed;
            Gravity = gravity;
            RotationSpeed = rotationSpeed;
        }

        public string Address { get; }

        public string DisplayName { get; }

        public float MoveSpeed { get; }

        public float Gravity { get; }

        public float RotationSpeed { get; }
    }
}
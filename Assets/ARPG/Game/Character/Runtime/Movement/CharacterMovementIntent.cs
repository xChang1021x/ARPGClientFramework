using UnityEngine;

namespace ARPG.Game.Character.Movement
{
    /// <summary>
    /// Character的世界空间移动意图。
    /// </summary>
    public readonly struct CharacterMovementIntent
    {
        public CharacterMovementIntent(
            Vector3 worldDirection)
        {
            worldDirection.y = 0f;

            WorldDirection =
                Vector3.ClampMagnitude(
                    worldDirection,
                    1f);
        }

        public Vector3 WorldDirection { get; }

        public bool HasMovement =>
            WorldDirection.sqrMagnitude >
            0.0001f;
    }
}
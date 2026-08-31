using UnityEngine;

namespace ARPG.Game.Character.Movement
{
    /// <summary>
    /// 描述调用方希望角色执行的移动。
    ///
    /// Intent只描述“想往哪里走”，
    /// 不负责真正修改Transform。
    /// </summary>
    public readonly struct CharacterMovementIntent
    {
        public CharacterMovementIntent(
            Vector2 move)
        {
            Move = Vector2.ClampMagnitude(
                move,
                1f);
        }

        /// <summary>
        /// 输入平面：
        ///
        /// X = 左右
        /// Y = 前后
        /// </summary>
        public Vector2 Move { get; }

        public bool HasMovement =>
            Move.sqrMagnitude > 0.0001f;
    }
}
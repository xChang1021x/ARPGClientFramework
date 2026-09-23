using System;
using ARPG.Game.Character.Movement;
using UnityEngine;

namespace ARPG.Game.Character.Player.Input
{
    /// <summary>
    /// 将玩家二维输入转换为基于摄像机方向的
    /// 世界空间Character Movement Intent。
    /// </summary>
    public sealed class PlayerMovementIntentBuilder
    {
        private readonly Transform _cameraTransform;

        public PlayerMovementIntentBuilder(
            Transform cameraTransform)
        {
            _cameraTransform =
                cameraTransform
                    ? cameraTransform
                    : throw new ArgumentNullException(
                        nameof(cameraTransform));
        }

        public CharacterMovementIntent Build(
            Vector2 input)
        {
            if (input.sqrMagnitude <= 0.0001f)
            {
                return new CharacterMovementIntent(
                    Vector3.zero);
            }

            Vector3 forward =
                _cameraTransform.forward;

            Vector3 right =
                _cameraTransform.right;

            /*
             * 只保留XZ平面。
             *
             * 摄像机如果向下俯视，
             * forward本身带Y分量，
             * 不能直接拿去移动角色。
             */
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 worldDirection =
                forward * input.y +
                right * input.x;

            return new CharacterMovementIntent(
                worldDirection);
        }
    }
}
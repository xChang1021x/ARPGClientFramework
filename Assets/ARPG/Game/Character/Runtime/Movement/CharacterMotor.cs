using System;
using UnityEngine;

namespace ARPG.Game.Character.Movement
{
    /// <summary>
    /// Character运动执行器。
    ///
    /// 负责：
    /// - 平面移动
    /// - CharacterController碰撞移动
    /// - 重力
    /// - Grounded状态
    ///
    /// 不负责：
    /// - 玩家输入
    /// - AI决策
    /// - 动画
    /// - 状态机
    /// </summary>
    public sealed class CharacterMotor
    {
        private const float GroundedVerticalVelocity =
            -2f;

        private readonly CharacterController
            _controller;

        private float _verticalVelocity;

        public CharacterMotor(
            CharacterController controller,
            float moveSpeed,
            float gravity)
        {
            _controller =
                controller
                ? controller
                : throw new ArgumentNullException(
                    nameof(controller));

            if (moveSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveSpeed));
            }

            if (gravity >= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gravity));
            }

            MoveSpeed = moveSpeed;
            Gravity = gravity;
        }

        public float MoveSpeed { get; }

        public float Gravity { get; }

        public bool IsGrounded =>
            _controller.isGrounded;

        public float VerticalVelocity =>
            _verticalVelocity;

        public void Tick(
            CharacterMovementIntent intent,
            float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime));
            }

            UpdateVerticalVelocity(
                deltaTime);

            Vector3 horizontalVelocity =
                CreateHorizontalVelocity(
                    intent);

            Vector3 velocity =
                horizontalVelocity +
                Vector3.up * _verticalVelocity;

            _controller.Move(
                velocity * deltaTime);
        }

        private Vector3 CreateHorizontalVelocity(
            CharacterMovementIntent intent)
        {
            if (!intent.HasMovement)
            {
                return Vector3.zero;
            }

            Vector3 direction =
                new Vector3(
                    intent.Move.x,
                    0f,
                    intent.Move.y);

            return direction *
                   MoveSpeed;
        }

        private void UpdateVerticalVelocity(
            float deltaTime)
        {
            /*
             * CharacterController.isGrounded
             * 只反映上一次Move后的碰撞结果。
             *
             * Grounded时保持一个小的向下速度，
             * 让Controller持续贴地。
             */
            if (_controller.isGrounded &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity =
                    GroundedVerticalVelocity;

                return;
            }

            _verticalVelocity +=
                Gravity * deltaTime;
        }
    }
}
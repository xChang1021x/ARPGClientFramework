using System;
using UnityEngine;

namespace ARPG.Game.Character.Movement
{
    /// <summary>
    /// Character的基础运动执行器。
    ///
    /// 负责：
    /// Movement Intent
    /// →
    /// World-space displacement。
    ///
    /// 不负责读取玩家输入。
    /// </summary>
    public sealed class CharacterMotor
    {
        private readonly Transform _transform;

        public CharacterMotor(
            Transform transform,
            float moveSpeed)
        {
            _transform =
                transform
                ? transform
                : throw new ArgumentNullException(
                    nameof(transform));

            if (moveSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(moveSpeed));
            }

            MoveSpeed = moveSpeed;
        }

        public float MoveSpeed { get; }

        public void Tick(
            CharacterMovementIntent intent,
            float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime));
            }

            if (!intent.HasMovement)
            {
                return;
            }

            Vector3 direction =
                new Vector3(
                    intent.Move.x,
                    0f,
                    intent.Move.y);

            Vector3 displacement =
                direction *
                MoveSpeed *
                deltaTime;

            _transform.position +=
                displacement;
        }
    }
}
using System;
using UnityEngine;

namespace ARPG.Framework.Pool
{
    /// <summary>
    /// 绑定在池化GameObject上的生命周期组件。
    /// 负责记录所属对象池并提供安全回收入口。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledObject : MonoBehaviour
    {
        private Action<GameObject> _releaseAction;

        public bool IsSpawned { get; private set; }

        internal void Initialize(Action<GameObject> releaseAction)
        {
            _releaseAction = releaseAction
                ?? throw new ArgumentNullException(nameof(releaseAction));
        }

        internal void MarkSpawned()
        {
            if (IsSpawned)
            {
                throw new InvalidOperationException(
                    $"GameObject '{name}' is already spawned.");
            }

            IsSpawned = true;
        }

        internal void MarkDespawned()
        {
            if (!IsSpawned)
            {
                throw new InvalidOperationException(
                    $"GameObject '{name}' is already despawned.");
            }

            IsSpawned = false;
        }

        /// <summary>
        /// 将当前GameObject归还到所属对象池。
        /// </summary>
        public void Release()
        {
            if (_releaseAction == null)
            {
                throw new InvalidOperationException(
                    $"GameObject '{name}' does not belong to a pool.");
            }

            _releaseAction.Invoke(gameObject);
        }
    }
}
using System;
using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// ResourceService内部缓存条目。
    ///
    /// 保存：
    /// 1. 实际资源对象；
    /// 2. 业务引用计数；
    /// 3. Provider对应的底层释放策略。
    /// </summary>
    internal sealed class ResourceEntry
    {
        private Action _releaseUnderlyingAction;

        public ResourceEntry(
            UnityEngine.Object asset,
            Action releaseUnderlyingAction)
        {
            Asset = asset != null
                ? asset
                : throw new ArgumentNullException(
                    nameof(asset));

            _releaseUnderlyingAction =
                releaseUnderlyingAction
                ?? throw new ArgumentNullException(
                    nameof(releaseUnderlyingAction));
        }

        /// <summary>
        /// 实际Unity资源。
        /// </summary>
        public UnityEngine.Object Asset { get; }

        /// <summary>
        /// 当前有多少ResourceHandle持有该资源。
        /// </summary>
        public int ReferenceCount { get; set; }

        /// <summary>
        /// 释放Provider底层资源。
        ///
        /// 此方法具有幂等性，
        /// 多次调用只会真正释放一次。
        /// </summary>
        public void ReleaseUnderlying()
        {
            if (_releaseUnderlyingAction == null)
            {
                return;
            }

            Action releaseAction =
                _releaseUnderlyingAction;

            _releaseUnderlyingAction = null;

            releaseAction.Invoke();
        }
    }
}
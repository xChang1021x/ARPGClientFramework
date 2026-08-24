using System;
using UnityEngine;

namespace ARPG.Game.Resource
{
    /// <summary>
    /// 表示一次资源持有关系。
    /// Dispose后资源句柄失效。
    /// </summary>
    public sealed class ResourceHandle<T>
        : IDisposable
        where T : UnityEngine.Object
    {
        private Action _releaseAction;

        internal ResourceHandle(
            T asset,
            Action releaseAction)
        {
            Asset = asset
                ? asset
                : throw new ArgumentNullException(
                    nameof(asset));

            _releaseAction =
                releaseAction
                ?? throw new ArgumentNullException(
                    nameof(releaseAction));
        }

        public T Asset { get; }

        public bool IsReleased =>
            _releaseAction == null;

        public void Dispose()
        {
            if (_releaseAction == null)
            {
                return;
            }

            Action releaseAction =
                _releaseAction;

            _releaseAction = null;

            releaseAction.Invoke();
        }
    }
}
using System;

namespace ARPG.Framework.Event
{
    /// <summary>
    /// 事件订阅凭证。
    /// Dispose时自动取消订阅。
    /// </summary>
    public sealed class EventSubscription : IDisposable
    {
        private Action _unsubscribeAction;

        internal EventSubscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction
                ?? throw new ArgumentNullException(nameof(unsubscribeAction));
        }

        public void Dispose()
        {
            _unsubscribeAction?.Invoke();
            _unsubscribeAction = null;
        }
    }
}
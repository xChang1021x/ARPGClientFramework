using System;
using System.Collections.Generic;

namespace ARPG.Framework.Event
{
    /// <summary>
    /// 强类型同步事件总线。
    /// 适用于模块之间的低频状态通知。
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        /// <typeparam name="TEvent">事件类型。</typeparam>
        /// <param name="handler">事件处理函数。</param>
        /// <returns>可用于自动取消订阅的凭证。</returns>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                Action<TEvent> existingHandlers = (Action<TEvent>)existingDelegate;

                // 避免同一个方法被重复注册。
                foreach (Delegate registeredHandler in existingHandlers.GetInvocationList())
                {
                    if (registeredHandler.Equals(handler))
                    {
                        return new EventSubscription(
                            () => Unsubscribe(handler));
                    }
                }

                _handlers[eventType] = existingHandlers + handler;
            }
            else
            {
                _handlers[eventType] = handler;
            }

            return new EventSubscription(
                () => Unsubscribe(handler));
        }

        /// <summary>
        /// 取消订阅指定类型的事件。
        /// </summary>
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                return;
            }

            Action<TEvent> existingHandlers = (Action<TEvent>)existingDelegate;
            Action<TEvent> remainingHandlers = existingHandlers - handler;

            if (remainingHandlers is null)
            {
                _handlers.Remove(eventType);
                return;
            }

            _handlers[eventType] = remainingHandlers;
        }

        /// <summary>
        /// 同步发布事件。
        /// </summary>
        public void Publish<TEvent>(TEvent eventData)
            where TEvent : IEvent
        {
            Type eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out Delegate existingDelegate))
            {
                return;
            }

            /*
             * 获取调用列表快照。
             * 即使某个监听者在回调中取消订阅，
             * 也不会破坏本次遍历。
             */
            Delegate[] invocationList =
                existingDelegate.GetInvocationList();

            foreach (Delegate callback in invocationList)
            {
                try
                {
                    ((Action<TEvent>)callback).Invoke(eventData);
                }
                catch (Exception exception)
                {
                    /*
                     * Framework层不应直接依赖UnityEngine.Debug。
                     * 当前先抛出异常，后续可以接入统一日志系统。
                     */
                    throw new EventDispatchException(
                        eventType,
                        callback.Method.Name,
                        exception);
                }
            }
        }

        /// <summary>
        /// 清除指定事件的所有监听者。
        /// 通常只用于测试或模块卸载。
        /// </summary>
        public void Clear<TEvent>()
            where TEvent : IEvent
        {
            _handlers.Remove(typeof(TEvent));
        }

        /// <summary>
        /// 清除全部事件监听。
        /// 不应在普通Gameplay流程中随意调用。
        /// </summary>
        public void ClearAll()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// 获取指定事件的监听者数量，便于测试和调试。
        /// </summary>
        public int GetSubscriberCount<TEvent>()
            where TEvent : IEvent
        {
            if (!_handlers.TryGetValue(
                    typeof(TEvent),
                    out Delegate existingDelegate))
            {
                return 0;
            }

            return existingDelegate.GetInvocationList().Length;
        }
    }

    /// <summary>
    /// 事件分发异常。
    /// 保存出错事件类型及监听方法信息。
    /// </summary>
    public sealed class EventDispatchException : Exception
    {
        public Type EventType { get; }

        public string HandlerName { get; }

        public EventDispatchException(
            Type eventType,
            string handlerName,
            Exception innerException)
            : base(
                $"Failed to dispatch event '{eventType.Name}' " +
                $"to handler '{handlerName}'.",
                innerException)
        {
            EventType = eventType;
            HandlerName = handlerName;
        }
    }
}
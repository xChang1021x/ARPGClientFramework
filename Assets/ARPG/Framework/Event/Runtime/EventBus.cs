using System;
using System.Collections.Generic;
using ARPG.Framework.Core;
using ARPG.Framework.Diagnostics;

namespace ARPG.Framework.Event
{
    /// <summary>
    /// 强类型同步事件总线。
    /// 适用于模块之间的低频状态通知。
    /// </summary>
    public sealed class EventBus : IShutdownable
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        private readonly IExceptionReporter _exceptionReporter;

        public EventBus(
    IExceptionReporter exceptionReporter = null)
        {
            _exceptionReporter = exceptionReporter;
        }

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
                        throw new InvalidOperationException(
    $"Handler '{handler.Method.Name}' has already subscribed to '{eventType.Name}'.");
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

            if (!_handlers.TryGetValue(
                    eventType,
                    out Delegate existingDelegate))
            {
                return;
            }

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
                    var dispatchException =
                        new EventDispatchException(
                            eventType,
                            callback.Method.Name,
                            exception);

                    if (_exceptionReporter != null)
                    {
                        _exceptionReporter.Report(
                            category: "EventBus",
                            message:
                                $"Failed to dispatch " +
                                $"'{eventType.Name}' to " +
                                $"'{callback.Method.Name}'.",
                            exception: dispatchException);

                        continue;
                    }

                    throw dispatchException;
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

        public void Shutdown()
        {
            ClearAll();
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
using System;
using System.Collections.Generic;

namespace ARPG.Framework.Timer
{
    /// <summary>
    /// 纯C#同步计时服务。
    /// 由外部每帧调用Tick驱动。
    /// </summary>
    public sealed class TimerService : IDisposable
    {
        private sealed class TimerTask
        {
            public float Interval;
            public float RemainingTime;

            /// <summary>
            /// 剩余执行次数。
            /// -1表示无限循环。
            /// </summary>
            public int RemainingExecutions;

            public bool UseUnscaledTime;
            public bool IsCancelled;

            public Action Callback;
            public TimerHandle Handle;
        }

        private readonly List<TimerTask> _tasks = new();
        private readonly List<TimerTask> _pendingTasks = new();

        private bool _isTicking;
        private bool _isDisposed;

        public int ActiveTimerCount =>
            _tasks.Count + _pendingTasks.Count;

        /// <summary>
        /// 创建一次性延迟任务。
        /// </summary>
        public TimerHandle Delay(
            float delay,
            Action callback,
            bool useUnscaledTime = false)
        {
            return Schedule(
                interval: delay,
                repeatCount: 1,
                callback: callback,
                useUnscaledTime: useUnscaledTime);
        }

        /// <summary>
        /// 创建循环任务。
        /// repeatCount为-1时无限循环。
        /// </summary>
        public TimerHandle Repeat(
            float interval,
            Action callback,
            int repeatCount = -1,
            bool useUnscaledTime = false)
        {
            return Schedule(
                interval,
                repeatCount,
                callback,
                useUnscaledTime);
        }

        /// <summary>
        /// 创建定时任务。
        /// </summary>
        public TimerHandle Schedule(
            float interval,
            int repeatCount,
            Action callback,
            bool useUnscaledTime = false)
        {
            ThrowIfDisposed();

            if (interval <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(interval),
                    "Timer interval must be greater than zero.");
            }

            if (repeatCount == 0 || repeatCount < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(repeatCount),
                    "Repeat count must be positive or -1 for infinite repetition.");
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            TimerTask task = null;

            TimerHandle handle =
                new TimerHandle(() =>
                {
                    if (task != null)
                    {
                        task.IsCancelled = true;
                    }
                });

            task = new TimerTask
            {
                Interval = interval,
                RemainingTime = interval,
                RemainingExecutions = repeatCount,
                UseUnscaledTime = useUnscaledTime,
                Callback = callback,
                Handle = handle
            };

            if (_isTicking)
            {
                /*
                 * 回调中新增的任务从下一帧开始计时，
                 * 避免修改正在遍历的集合。
                 */
                _pendingTasks.Add(task);
            }
            else
            {
                _tasks.Add(task);
            }

            return handle;
        }

        /// <summary>
        /// 每帧更新时间。
        /// </summary>
        /// <param name="deltaTime">受Time.timeScale影响的时间。</param>
        /// <param name="unscaledDeltaTime">不受Time.timeScale影响的时间。</param>
        public void Tick(
            float deltaTime,
            float unscaledDeltaTime)
        {
            ThrowIfDisposed();

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime));
            }

            if (unscaledDeltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(unscaledDeltaTime));
            }

            _isTicking = true;

            try
            {
                for (int index = _tasks.Count - 1;
                     index >= 0;
                     index--)
                {
                    TimerTask task = _tasks[index];

                    if (task.IsCancelled)
                    {
                        RemoveTaskAt(index, markCompleted: true);
                        continue;
                    }

                    float elapsedTime =
                        task.UseUnscaledTime
                            ? unscaledDeltaTime
                            : deltaTime;

                    task.RemainingTime -= elapsedTime;

                    if (task.RemainingTime > 0f)
                    {
                        continue;
                    }

                    ExecuteTask(task);

                    if (task.IsCancelled)
                    {
                        RemoveTaskAt(index, markCompleted: true);
                        continue;
                    }

                    if (task.RemainingExecutions > 0)
                    {
                        task.RemainingExecutions--;
                    }

                    if (task.RemainingExecutions == 0)
                    {
                        RemoveTaskAt(index, markCompleted: true);
                        continue;
                    }

                    /*
                     * 使用加法而不是直接赋值，
                     * 可以减少帧率波动造成的累计误差。
                     */
                    task.RemainingTime += task.Interval;

                    /*
                     * 本帧时间跨度可能远大于Interval。
                     * 当前版本最多执行一次回调，
                     * 避免卡顿后单帧集中补执行大量任务。
                     */
                    if (task.RemainingTime <= 0f)
                    {
                        task.RemainingTime = task.Interval;
                    }
                }
            }
            finally
            {
                _isTicking = false;
                FlushPendingTasks();
            }
        }

        /// <summary>
        /// 取消全部计时任务。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            foreach (TimerTask task in _tasks)
            {
                task.IsCancelled = true;
                task.Handle.MarkCompleted();
            }

            foreach (TimerTask task in _pendingTasks)
            {
                task.IsCancelled = true;
                task.Handle.MarkCompleted();
            }

            if (_isTicking)
            {
                _pendingTasks.Clear();
                return;
            }

            _tasks.Clear();
            _pendingTasks.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Clear();
            _isDisposed = true;
        }

        private static void ExecuteTask(TimerTask task)
        {
            try
            {
                task.Callback.Invoke();
            }
            catch (Exception exception)
            {
                throw new TimerCallbackException(
                    task.Callback.Method.Name,
                    exception);
            }
        }

        private void RemoveTaskAt(
            int index,
            bool markCompleted)
        {
            TimerTask task = _tasks[index];

            if (markCompleted)
            {
                task.Handle.MarkCompleted();
            }

            int lastIndex = _tasks.Count - 1;

            /*
             * 使用尾部元素覆盖当前元素，
             * 避免List.RemoveAt造成后续元素整体移动。
             */
            _tasks[index] = _tasks[lastIndex];
            _tasks.RemoveAt(lastIndex);
        }

        private void FlushPendingTasks()
        {
            if (_pendingTasks.Count == 0)
            {
                return;
            }

            foreach (TimerTask task in _pendingTasks)
            {
                if (task.IsCancelled)
                {
                    task.Handle.MarkCompleted();
                    continue;
                }

                _tasks.Add(task);
            }

            _pendingTasks.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    nameof(TimerService));
            }
        }
    }

    /// <summary>
    /// 定时任务回调异常。
    /// </summary>
    public sealed class TimerCallbackException : Exception
    {
        public string CallbackName { get; }

        public TimerCallbackException(
            string callbackName,
            Exception innerException)
            : base(
                $"Timer callback '{callbackName}' failed.",
                innerException)
        {
            CallbackName = callbackName;
        }
    }
}
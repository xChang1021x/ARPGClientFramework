using System;

namespace ARPG.Framework.Timer
{
    /// <summary>
    /// 定时任务句柄。
    /// 用于取消任务并查询运行状态。
    /// </summary>
    public sealed class TimerHandle : IDisposable
    {
        private Action _cancelAction;

        internal TimerHandle(Action cancelAction)
        {
            _cancelAction = cancelAction
                ?? throw new ArgumentNullException(nameof(cancelAction));
        }

        /// <summary>
        /// 当前任务是否已结束或被取消。
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// 取消当前定时任务。
        /// 重复调用不会产生副作用。
        /// </summary>
        public void Cancel()
        {
            if (IsCompleted)
            {
                return;
            }

            IsCompleted = true;

            _cancelAction?.Invoke();
            _cancelAction = null;
        }

        public void Dispose()
        {
            Cancel();
        }

        internal void MarkCompleted()
        {
            IsCompleted = true;
            _cancelAction = null;
        }
    }
}
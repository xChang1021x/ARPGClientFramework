using System;

namespace ARPG.Framework.Logging
{
    /// <summary>
    /// 日志输出接口。
    /// 具体输出目标由上层实现。
    /// </summary>
    public interface ILogger
    {
        void Log(
            LogLevel level,
            string category,
            string message,
            Exception exception = null);
    }
}
using System;

namespace ARPG.Framework.Logging
{
    /// <summary>
    /// 不执行任何输出的Logger。
    /// 可用于关闭日志或测试环境。
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        public void Log(
            LogLevel level,
            string category,
            string message,
            Exception exception = null)
        {
        }
    }
}
using System;

namespace ARPG.Framework.Diagnostics
{
    /// <summary>
    /// 框架异常上报接口。
    /// 负责接收非致命运行时异常。
    /// </summary>
    public interface IExceptionReporter
    {
        void Report(
            string category,
            string message,
            Exception exception);
    }
}
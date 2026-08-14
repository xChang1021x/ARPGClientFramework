using System;
using ARPG.Framework.Logging;

namespace ARPG.Framework.Diagnostics
{
    /// <summary>
    /// 使用LogService输出异常的上报器。
    /// </summary>
    public sealed class LoggingExceptionReporter
        : IExceptionReporter
    {
        private readonly LogService _logService;

        public LoggingExceptionReporter(
            LogService logService)
        {
            _logService = logService
                ?? throw new ArgumentNullException(
                    nameof(logService));
        }

        public void Report(
            string category,
            string message,
            Exception exception)
        {
            _logService.Error(
                category,
                message,
                exception);
        }
    }
}
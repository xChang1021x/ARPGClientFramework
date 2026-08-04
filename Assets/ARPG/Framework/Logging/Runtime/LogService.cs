using System;

namespace ARPG.Framework.Logging
{
    /// <summary>
    /// 日志服务。
    /// 负责日志等级过滤和统一调用入口。
    /// </summary>
    public sealed class LogService
    {
        private ILogger _logger;

        public LogLevel MinimumLevel { get; set; }

        public LogService(
            ILogger logger,
            LogLevel minimumLevel = LogLevel.Debug)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            MinimumLevel = minimumLevel;
        }

        /// <summary>
        /// 替换日志输出实现。
        /// 主要用于测试或运行环境切换。
        /// </summary>
        public void SetLogger(ILogger logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Trace(
            string category,
            string message)
        {
            Write(
                LogLevel.Trace,
                category,
                message);
        }

        public void Debug(
            string category,
            string message)
        {
            Write(
                LogLevel.Debug,
                category,
                message);
        }

        public void Info(
            string category,
            string message)
        {
            Write(
                LogLevel.Info,
                category,
                message);
        }

        public void Warning(
            string category,
            string message)
        {
            Write(
                LogLevel.Warning,
                category,
                message);
        }

        public void Error(
            string category,
            string message,
            Exception exception = null)
        {
            Write(
                LogLevel.Error,
                category,
                message,
                exception);
        }

        public void Fatal(
            string category,
            string message,
            Exception exception = null)
        {
            Write(
                LogLevel.Fatal,
                category,
                message,
                exception);
        }

        public void Write(
            LogLevel level,
            string category,
            string message,
            Exception exception = null)
        {
            if (level < MinimumLevel ||
                MinimumLevel == LogLevel.None)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                category = "General";
            }

            if (message == null)
            {
                message = string.Empty;
            }

            _logger.Log(
                level,
                category,
                message,
                exception);
        }
    }
}
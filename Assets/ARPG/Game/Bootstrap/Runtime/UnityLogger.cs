using System;
using ARPG.Framework.Logging;
using UnityEngine;

namespace ARPG.Game.Bootstrap
{
    /// <summary>
    /// 基于UnityEngine.Debug的日志输出实现。
    /// </summary>
    public sealed class UnityLogger : Framework.Logging.ILogger
    {
        public void Log(
            LogLevel level,
            string category,
            string message,
            Exception exception = null)
        {
            string formattedMessage =
                FormatMessage(
                    level,
                    category,
                    message,
                    exception);

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(
                        formattedMessage);
                    break;

                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(
                        formattedMessage);
                    break;

                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(
                        formattedMessage);
                    break;

                case LogLevel.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(level),
                        level,
                        "Unsupported log level.");
            }
        }

        private static string FormatMessage(
            LogLevel level,
            string category,
            string message,
            Exception exception)
        {
            if (exception == null)
            {
                return
                    $"[{level}][{category}] {message}";
            }

            return
                $"[{level}][{category}] {message}\n" +
                $"{exception}";
        }
    }
}
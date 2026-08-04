namespace ARPG.Framework.Logging
{
    /// <summary>
    /// 日志严重程度。
    /// 数值越大，严重程度越高。
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5,
        None = 6
    }
}
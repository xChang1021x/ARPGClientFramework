using ARPG.Framework.Event;

namespace ARPG.Game.Tests.Diagnostics
{
    /// <summary>
    /// 基础设施异常隔离测试事件。
    /// </summary>
    public readonly struct DiagnosticsTestEvent : IEvent
    {
        public int TestId { get; }

        public DiagnosticsTestEvent(int testId)
        {
            TestId = testId;
        }
    }
}
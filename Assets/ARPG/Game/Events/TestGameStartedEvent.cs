using ARPG.Framework.Event;

namespace ARPG.Game.Events
{
    public readonly struct TestGameStartedEvent : IEvent
    {
        public string Message { get; }

        public TestGameStartedEvent(string message)
        {
            Message = message;
        }
    }
}
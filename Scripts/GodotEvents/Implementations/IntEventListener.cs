using Godot;

namespace BushyCore
{
    public partial class IntEventListener : BaseEventListener<int>
    {
        [Signal] public delegate void EventReceivedEventHandler(int newValue);
        protected override void HandleEvent(int value)
        {
            EmitSignal(SignalName.EventReceived, value);
        }
    }
}
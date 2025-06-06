using Godot;

namespace BushyCore
{
    public partial class Vector2EventListener : BaseEventListener<Vector2>
    {
        [Signal] public delegate void EventReceivedEventHandler(Vector2 newValue);
        protected override void HandleEvent(Vector2 value)
        {
            EmitSignal(SignalName.EventReceived, value);
        }
    }
}
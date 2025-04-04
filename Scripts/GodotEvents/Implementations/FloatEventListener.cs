using Godot;

namespace BushyCore
{
    public partial class FloatEventListener : BaseEventListener<float>
    {
        [Signal] public delegate void EventReceivedEventHandler(float newValue);
        protected override void HandleEvent(float value)
        {
            GD.Print("ASS");
            EmitSignal(SignalName.EventReceived, value);
        }
    }
}
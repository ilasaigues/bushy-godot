using Godot;

namespace BushyCore
{
    [Tool]
    [GlobalClass]
    public partial class FloatEvent : BaseEventResource<float>
    {
        [Export]
        protected override float _editorValue { get; set; }
        [Export]
        private bool CallWithEditorValue
        {
            get { return false; }
            set
            {
                TriggerEvent(_editorValue);
            }
        }
    }
}
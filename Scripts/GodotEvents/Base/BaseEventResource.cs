using Godot;
using System;
namespace BushyCore
{
    [Tool]
    public abstract partial class BaseEventResource<[MustBeVariant] T> : Resource
    {
        [Export(PropertyHint.MultilineText)]
        private string _developerDescription;

        public delegate void EventHandler<T>(T item);

        private event EventHandler<T> _onEventTriggered;

        public event EventHandler<T> OnEventTriggered
        {
            add
            {
                _onEventTriggered ??= a => { };
                GD.Print("ADDED");
                _onEventTriggered += value;
            }
            remove { _onEventTriggered -= value; }
        }

        protected abstract T _editorValue { get; set; }

        protected void TestWithEditorValue()
        {
            if (Engine.IsEditorHint()) return;
            GD.Print("BOLAS");
            TriggerEvent(_editorValue);
        }

        public void TriggerEvent(T value)
        {
            if (Engine.IsEditorHint()) return;
            GD.Print("TRIGGERING EVENT");
            _onEventTriggered ??= a => { };
            _onEventTriggered.Invoke(value);
        }
    }

}
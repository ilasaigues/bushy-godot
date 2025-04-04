using Godot;
using System;

namespace BushyCore
{

    public abstract partial class BaseEventListener<[MustBeVariant] T> : Node
    {
        [Export] public BaseEventResource<T> EventResource;

        public override void _Ready()
        {
            SubscribeToEvent();
        }

        private void SubscribeToEvent()
        {
            if (EventResource != null)
            {
                EventResource.OnEventTriggered += HandleEvent;
            }
        }

        private void UnsubscribeFromEvent()
        {
            if (EventResource != null)
            {
                EventResource.OnEventTriggered -= HandleEvent;
            }
        }

        protected abstract void HandleEvent(T value);

        public void SetResource(BaseEventResource<T> newResource)
        {
            UnsubscribeFromEvent();
            EventResource = newResource;
            SubscribeToEvent();
        }

        public override void _ExitTree()
        {
            UnsubscribeFromEvent();
        }
    }


}

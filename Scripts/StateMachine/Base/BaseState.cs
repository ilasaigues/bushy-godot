using Godot;
using System;

namespace BushyCore
{
    public abstract partial class BaseState<T> : Node, IState<T> where T : Node
    {
        public bool Active { get; set; }
        public abstract T Agent { get; set; }
        public double TimeSinceStateStart { get; set; }

        public virtual bool TryChangeToState(Type type, params StateConfig.IBaseStateConfig[] configs)
        {
            return GetType() == type;
        }

        public void EnterState(params StateConfig.IBaseStateConfig[] configs)
        {
            if (Active)
            {
                return;
            }
            Active = true;
            EnterStateInternal(configs);
        }

        protected abstract void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs);
        public StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta)
        {
            TimeSinceStateStart += delta;
            return ProcessStateInternal(prevStatus, delta);
        }

        protected abstract StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta);

        public void ExitState()
        {
            if (!Active)
            {
                return;
            }
            Active = false;
            ExitStateInternal();
        }
        protected abstract void ExitStateInternal();

        public virtual StateAnimationLevel UpdateAnimation()
        {
            return StateAnimationLevel.Regular;
        }
        public virtual void OnRigidBodyInteraction(Node2D node, bool enter) { }
        public virtual void OnAreaChange(Area2D area, bool enter) { }
        public virtual void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action) { }
        public virtual void OnInputAxisChanged(InputAxis axis) { }
    }
}
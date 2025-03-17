using Godot;
using System;

namespace BushyCore
{
    public abstract partial class BaseState<T> : Node, IState<T> where T : Node
    {
        public bool Active { get; set; }
        public T Agent { get; set; }
        public double TimeSinceStateStart { get; set; }

        public virtual void SetAgent(T newAgent)
        {
            Agent = newAgent;
        }

        public virtual bool TryChangeToState(Type type, params StateConfig.IBaseStateConfig[] configs) => GetType() == type;

        public virtual void EnterState(params StateConfig.IBaseStateConfig[] configs)
        {
            if (Active)
            {
                return;
            }
            GD.Print("Entering State: " + GetType());
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

        public virtual StateAnimationLevel UpdateAnimation() => StateAnimationLevel.Regular;
        public virtual bool OnRigidBodyInteraction(Node2D node, bool enter) => true;
        public virtual bool OnAreaChange(Area2D area, bool enter) => true;
        public virtual bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            return true;

        }
        public virtual bool OnInputAxisChanged(InputAxis axis) => true;
    }
}
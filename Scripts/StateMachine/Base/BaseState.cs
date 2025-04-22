using Godot;
using System;

namespace BushyCore
{
    public abstract partial class BaseState<T> : Node, IState<T> where T : Node
    {
        public bool Active { get; set; }
        public T Agent { get; set; }
        private DateTime _timeOfActivation;

        public TimeSpan TimeSinceStateEntered => DateTime.Now - _timeOfActivation;

        public virtual void SetAgent(T newAgent)
        {
            Agent = newAgent;
        }

        public virtual bool CanEnterState(Type type, params StateConfig.IBaseStateConfig[] configs) => type == GetType();


        public virtual void EnterState(Type type, params StateConfig.IBaseStateConfig[] configs)
        {
            if (type != GetType())
            {
                return;
            }
            Active = true;
            _timeOfActivation = DateTime.Now;
            GD.Print("Entering state: " + GetType().Name);
            EnterStateInternal(configs);
        }

        protected abstract void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs);
        public StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta)
        {
            if (Active && prevStatus.StateExecutionResult == StateExecutionResult.Continue)
                return ProcessStateInternal(prevStatus, delta);
            return prevStatus;
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
        public bool OnRigidBodyInteraction(Node2D node, bool enter) => OnRigidBodyInteractionInternal(node, enter);
        protected virtual bool OnRigidBodyInteractionInternal(Node2D node, bool enter) => true;
        public bool OnAreaChange(Area2D area, bool enter) => OnAreaChangeInternal(area, enter);
        protected virtual bool OnAreaChangeInternal(Area2D area, bool enter) => true;
        public bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action) => OnInputButtonChangedInternal(actionType, Action);
        protected virtual bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction Action) => true;
        public bool OnInputAxisChanged(InputAxis axis) => OnInputAxisChangedInternal(axis);
        protected virtual bool OnInputAxisChangedInternal(InputAxis axis) => true;
        public virtual string GetStateName() => GetType().Name;
    }
}
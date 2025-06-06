using Godot;
using System;

namespace BushyCore
{
    public interface IState
    {
        public bool Active { get; set; }
        bool CanEnterState(Type type, params StateConfig.IBaseStateConfig[] configs);
        void EnterState(Type type, params StateConfig.IBaseStateConfig[] configs);
        StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta);
        void ExitState();
        bool OnRigidBodyInteraction(Node2D node, bool enter);
        bool OnAreaChange(Area2D area, bool enter);
        bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action);
        bool OnInputAxisChanged(InputAxis axis);
        string GetStateName();
        TimeSpan TimeSinceStateEntered { get; }
    }

    public interface INullState : IState { }

    public interface IState<T> : IState where T : Node
    {
        T Agent { get; set; }
    }
}
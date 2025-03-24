using Godot;
using System;

namespace BushyCore
{
    public interface IState
    {
        public bool Active { get; set; }
        void EnterState(params StateConfig.IBaseStateConfig[] configs);
        StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta);
        void ExitState();
        double TimeSinceStateStart { get; set; }
        bool OnRigidBodyInteraction(Node2D node, bool enter);
        bool OnAreaChange(Area2D area, bool enter);
        bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action);
        bool OnInputAxisChanged(InputAxis axis);
        bool TryChangeToState(Type type, params StateConfig.IBaseStateConfig[] configs);

    }
    public interface IState<T> : IState where T : Node
    {
        T Agent { get; set; }
    }
}
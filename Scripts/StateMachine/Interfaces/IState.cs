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
        void OnRigidBodyInteraction(Node2D node, bool enter);
        void OnAreaChange(Area2D area, bool enter);
        void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action);
        void OnInputAxisChanged(InputAxis axis);
        bool TryChangeToState(Type type, params StateConfig.IBaseStateConfig[] configs);

    }
    public interface IState<T> : IState where T : Node
    {
        T Agent { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace BushyCore
{
    public class StateMachine<TAgent> where TAgent : Node
    {
        private TAgent Agent;
        public StateExecutionStatus MachineState { get; set; }

        [Export] private List<BaseState<TAgent>> States = new();
        private BaseState<TAgent> _currentState;

        private List<DisposableBinding> _bindings = new();

        public StateMachine(TAgent agent)
        {
            Agent = agent;
        }

        public void BindInput(DisposableBinding newBinding)
        {
            _bindings.Add(newBinding);
        }

        public void ClearInputBindings()
        {
            _bindings.ForEach(binding => binding.Dispose());
        }

        public void RegisterState<TState>(TState state) where TState : BaseState<TAgent>
        {
            States.Add(state);
        }

        public bool SetState(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            if (stateType.GetInterface(typeof(IState<TAgent>).Name) == null)
            {
                return true;
            }

            foreach (var state in States)
            {
                if (state != _currentState && state.TryChangeToState(stateType))
                {
                    _currentState?.ExitState();
                    _currentState = state;
                    GD.PushWarning("Entering State: " + state.GetType());
                    _currentState.EnterState(configs);
                    return true;
                }
            }

            return false;
        }

        public void OnRigidBodyInteraction(Node2D body, bool enter)
        {
            TryOrInterrupt(
                () => _currentState?.OnRigidBodyInteraction(body, enter)
                );
        }

        public void OnArea2DInteraction(Area2D area, bool enter)
        {
            TryOrInterrupt(
                () => _currentState?.OnRigidBodyInteraction(area, enter)
                );
        }

        public void UpdateStateInput(InputAction.InputActionType actionType, InputAction inputAction)
        {

            TryOrInterrupt(
                () => _currentState?.OnInputButtonChanged(actionType, inputAction)
                );

        }

        public void UpdateInputAxis(InputAxis axis)
        {
            TryOrInterrupt(
                () => _currentState?.OnInputAxisChanged(axis)
                );

        }

        private void TryOrInterrupt(Action handler)
        {
            try
            {
                handler();
            }
            catch (StateInterrupt interrupt)
            {
                SetState(interrupt.NextStateType, interrupt.Configs);
            }
        }

        public StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta)
        {
            if (_currentState != null && prevStatus.AnimationLevel != StateAnimationLevel.Uninterruptible)
            {
                prevStatus.AnimationLevel |= _currentState.UpdateAnimation();
            }
            if (prevStatus.StateExecutionResult == StateExecutionResult.Block)
            {
                return prevStatus;
            }

            try
            {
                return _currentState?.ProcessState(prevStatus, delta) ?? prevStatus;
            }
            catch (StateInterrupt ex)
            {
                SetState(ex.NextStateType, ex.Configs);
                return prevStatus;
            }
        }
    }
}
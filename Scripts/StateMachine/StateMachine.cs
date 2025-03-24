using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace BushyCore
{
    public abstract partial class StateMachine<TAgent> : Node where TAgent : Node
    {
        private TAgent Agent;
        public StateExecutionStatus MachineState { get; set; }

        private List<BaseState<TAgent>> States = new();
        private BaseState<TAgent> _currentState;

        public override void _Ready()
        {
            States = GetChildren().OfType<BaseState<TAgent>>().ToList();
        }

        public void SetAgent(TAgent agent)
        {
            Agent = agent;
            foreach (var state in States)
            {
                state.SetAgent(agent);
            }
        }

        public bool SetState(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            if (stateType.GetInterface(typeof(IState<TAgent>).Name) == null)
            {
                return true;
            }
            foreach (var state in States)
            {
                if (state.TryChangeToState(stateType))
                {
                    state.EnterState(configs);
                    if (state != _currentState)
                    {
                        _currentState?.ExitState();
                        _currentState = state;
                    }
                    return true;
                }
            }

            return false;
        }

        public bool OnRigidBodyInteraction(Node2D body, bool enter)
        {
            return TryReturnOrInterrupt(
                () => _currentState == null || _currentState.OnRigidBodyInteraction(body, enter)
            );
        }

        public bool OnArea2DInteraction(Area2D area, bool enter)
        {
            return TryReturnOrInterrupt(
               () => _currentState == null || _currentState.OnRigidBodyInteraction(area, enter)
            );
        }

        public bool UpdateStateInput(InputAction.InputActionType actionType, InputAction inputAction)
        {
            return TryReturnOrInterrupt(
               () => _currentState == null || _currentState.OnInputButtonChanged(actionType, inputAction)
           );
        }

        public bool UpdateInputAxis(InputAxis axis)
        {
            return TryReturnOrInterrupt(
               () => _currentState == null || _currentState.OnInputAxisChanged(axis)
           );
        }

        private bool TryReturnOrInterrupt(Func<bool> handler)
        {
            try
            {
                // Return the result of the handler
                return handler();
            }
            catch (StateInterrupt interrupt)
            {
                StopCurrentState();
                if (!SetState(interrupt.NextStateType, interrupt.Configs))
                {
                    throw;
                }
                return true;
            }
        }

        private void StopCurrentState()
        {
            _currentState.ExitState();
            _currentState = null;
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
                StopCurrentState();
                if (!SetState(ex.NextStateType, ex.Configs))
                {
                    throw;
                }
                return prevStatus;
            }
        }
    }
}
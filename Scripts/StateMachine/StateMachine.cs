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

        [Export] public bool OverrideLower { get; private set; }

        private Dictionary<Type, BaseState<TAgent>> States;
        private BaseState<TAgent> _currentState;

        public override void _Ready()
        {
            States = GetChildren().OfType<BaseState<TAgent>>().ToDictionary(child => child.GetType());
        }

        public void SetAgent(TAgent agent)
        {
            Agent = agent;
            foreach (var state in States.Values)
            {
                state.SetAgent(agent);
            }
        }

        public bool SetState(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            if (stateType.GetInterface(typeof(IState<TAgent>).Name) == null)
            {
                throw new TypeLoadException();
            }

            foreach (var nextState in States.Values)
            {
                if (nextState.CanEnterState(stateType, configs))
                {
                    if (nextState.Active && nextState is IParentState parentState)
                    {
                        nextState.EnterState(stateType, configs);
                    }
                    else
                    {
                        if (stateType != _currentState?.GetType())
                        {
                            _currentState?.ExitState();
                            _currentState = nextState;
                            nextState.EnterState(stateType, configs);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void UnsetState()
        {
            _currentState?.ExitState();
            _currentState = null;
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
                if (interrupt.StopStateMachine)
                {
                    UnsetState();
                }
                if (!SetState(interrupt.NextStateType, interrupt.Configs))
                {
                    throw;
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public StateExecutionStatus ProcessState(StateExecutionStatus prevStatus, double delta)
        {
            if (_currentState is { Active: false })
            {
                _currentState = null;
            }
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
                if (ex.StopStateMachine)
                {
                    UnsetState();
                }
                if (!SetState(ex.NextStateType, ex.Configs))
                {
                    throw;
                }
                return prevStatus;
            }
            catch
            {
                throw;
            }
        }

        public string GetCurrentStateName()
        {
            return _currentState?.GetStateName();
        }
    }
}
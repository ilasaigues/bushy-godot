using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static BushyCore.StateConfig;

namespace BushyCore
{
    public abstract partial class BaseParentState<TAgent, TParentState> : BaseState<TAgent>, IParentState<TAgent, TParentState>
        where TAgent : Node
        where TParentState : BaseParentState<TAgent, TParentState>
    {
        public IChildState<TAgent, TParentState> CurrentSubState { get; set; }

        public Dictionary<Type, BaseChildState<TAgent, TParentState>> SubStates { get; set; } = [];

        public override void _Ready()
        {
            SubStates = GetChildren().OfType<BaseChildState<TAgent, TParentState>>()
                .ToDictionary(child => child.GetType());
        }

        public override bool CanEnterState(Type type, params IBaseStateConfig[] configs)
        {
            return SubStates.ContainsKey(type);
        }


        public override void EnterState(Type type, params IBaseStateConfig[] configs)
        {
            base.EnterState(GetType(), configs);
            if (SubStates.ContainsKey(type))
            {
                SetSubstate(type, configs);
            }
        }


        public override void SetAgent(TAgent newAgent)
        {
            base.SetAgent(newAgent);
            foreach (var subState in SubStates.Values)
            {
                subState.SetAgent(newAgent);
                subState.ParentState = (TParentState)this;
            }
        }

        private bool SetSubstate(Type nextStateType, params IBaseStateConfig[] configs)
        {
            if (CurrentSubState?.GetType() == nextStateType)
            {
                return true;
            }
            else
            {
                if (SubStates.TryGetValue(nextStateType, out var nextState))
                {
                    if (nextState.CanEnterState(nextStateType, configs))
                    {
                        CurrentSubState?.ExitState();
                        CurrentSubState = nextState;
                        nextState.EnterState(nextStateType, configs);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override bool OnRigidBodyInteractionInternal(Node2D body, bool enter)
        {
            return TryOrThrow(() => CurrentSubState.OnRigidBodyInteraction(body, enter));
        }


        protected override bool OnAreaChangeInternal(Area2D area, bool enter)
        {
            return TryOrThrow(() => CurrentSubState.OnAreaChange(area, enter));
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction inputAction)
        {
            return TryOrThrow(() => CurrentSubState.OnInputButtonChanged(actionType, inputAction));
        }

        protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            return TryOrThrow(() => CurrentSubState.OnInputAxisChanged(axis));
        }

        public StateExecutionStatus ProcessSubState(StateExecutionStatus processConfig, double delta)
        {
            return TryOrThrow(() => CurrentSubState.ProcessState(processConfig, delta));
        }

        private T TryOrThrow<T>(Func<T> operation)
        {
            try
            {
                return operation();
            }
            catch (StateInterrupt interrupt)
            {
                if (!SetSubstate(interrupt.NextStateType, interrupt.Configs))
                {
                    throw;
                }
                return default;
            }
        }

        protected override void ExitStateInternal()
        {
            CurrentSubState.ExitState();
            CurrentSubState = null;
        }

        public override string GetStateName()
        {
            return base.GetStateName() + "." + CurrentSubState?.GetStateName();
        }

    }
}
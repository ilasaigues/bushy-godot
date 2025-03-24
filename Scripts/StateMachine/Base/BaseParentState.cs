using System;
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

        [Export] public BaseState<TAgent>[] SubStates { get; set; }

        private IChildState<TAgent, TParentState> _nextState = null;

        public override void EnterState(params IBaseStateConfig[] configs)
        {
            base.EnterState(configs);
            if (_nextState != null)
            {
                CurrentSubState?.ExitState();
                _nextState.EnterState(configs);
                CurrentSubState = _nextState;
            }
        }

        public override bool TryChangeToState(Type type, params IBaseStateConfig[] configs)
        {
            foreach (var subState in SubStates)
            {
                if (subState.GetType() == type && subState is IChildState<TAgent, TParentState> childState)
                {
                    _nextState = childState;
                    return true;
                }
            }
            return false;
        }

        public override void SetAgent(TAgent newAgent)
        {
            base.SetAgent(newAgent);
            SubStates = GetChildren()
            .Where(state => state is IChildState<TAgent, TParentState>)
            .Select(state => (BaseState<TAgent>)state)
            .ToArray();
            foreach (var subState in SubStates)
            {
                ((IChildState<TAgent, TParentState>)subState).ParentState = (TParentState)this;
                subState.SetAgent(Agent);
            }
        }

        public StateExecutionStatus ProcessSubState(StateExecutionStatus processConfig, double delta)
        {
            return CurrentSubState.ProcessState(processConfig, delta);
        }

        public void ExitSubState()
        {
            CurrentSubState.ExitState();
        }
    }
}
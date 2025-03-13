using Godot;
using System;
using System.Collections.Generic;
using static BushyCore.StateConfig;

namespace BushyCore
{
    public interface IParentState<TAgent, TParentState> : IState<TAgent>
        where TAgent : Node
        where TParentState : IParentState<TAgent, TParentState>
    {
        [Export] BaseState<TAgent>[] SubStates { get; set; }
        IChildState<TAgent, TParentState> CurrentSubState { get; set; }
        void TryEnterSubState(params IBaseStateConfig[] stateConfigs);
        StateExecutionStatus ProcessSubState(StateExecutionStatus processConfig, double delta);
        void ExitSubState();
    }
}
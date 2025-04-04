using Godot;
using System;
using System.Collections.Generic;
using static BushyCore.StateConfig;

namespace BushyCore
{
    public interface IParentState
    {
        StateExecutionStatus ProcessSubState(StateExecutionStatus processConfig, double delta);
    }
    public interface IParentState<TAgent, TParentState> : IParentState, IState<TAgent>
        where TAgent : Node
        where TParentState : IParentState<TAgent, TParentState>
    {
        public Dictionary<Type, BaseChildState<TAgent, TParentState>> SubStates { get; set; }
        IChildState<TAgent, TParentState> CurrentSubState { get; set; }
    }
}
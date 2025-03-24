using Godot;
using System;

namespace BushyCore
{
    public interface IBaseChildState
    {
        public Type ParentStateType { get; }
    }
    public interface IChildState<TAgent, TParentState> : IState<TAgent>, IBaseChildState
        where TAgent : Node
        where TParentState : IParentState<TAgent, TParentState>
    {
        [Export] TParentState ParentState { get; set; }
        Type IBaseChildState.ParentStateType => typeof(TParentState);
    }
}
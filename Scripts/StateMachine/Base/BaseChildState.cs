using Godot;

namespace BushyCore
{
    public abstract partial class BaseChildState<TAgent, TParentState> : BaseState<TAgent>, IChildState<TAgent, TParentState>
    where TAgent : Node
    where TParentState : IParentState<TAgent, TParentState>
    {
        public TParentState ParentState { get; set; }
    }
}
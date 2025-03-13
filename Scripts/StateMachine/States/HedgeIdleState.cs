
using Godot;
using GodotUtilities;

namespace BushyCore
{
    public partial class HedgeIdleState : BasePlayerState, IChildState<PlayerController, HedgeState>
    {
        public HedgeState ParentState { get; set; }

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
        }

        protected override void ExitStateInternal()
        {
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            return prevStatus;
        }
    }
}
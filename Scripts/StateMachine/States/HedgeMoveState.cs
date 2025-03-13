
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeMoveState : BasePlayerState, IChildState<PlayerController, HedgeState>
    {
        public HedgeState ParentState { get; set; }

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            ParentState.Direction = Agent.MovementInputVector;

            ParentState.yAxisMovement.HandleMovement(delta);
            ParentState.xAxisMovement.HandleMovement(delta);

            VelocityUpdate();
            return ParentState.ProcessState(prevStatus, delta);
        }

        protected void VelocityUpdate()
        {
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float)ParentState.xAxisMovement.Velocity, (float)ParentState.yAxisMovement.Velocity);
        }

        protected override void ExitStateInternal()
        {
        }
    }
}
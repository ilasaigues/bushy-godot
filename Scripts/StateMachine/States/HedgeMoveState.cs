
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeMoveState : BaseChildState<PlayerController, HedgeParentState>
    {

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            GD.Print("BALLS");
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            GD.Print("ASS");
            ParentState.Direction = Agent.MovementInputVector;

            ParentState.yAxisMovement.HandleMovement(delta);
            ParentState.xAxisMovement.HandleMovement(delta);

            VelocityUpdate();
            return prevStatus;
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
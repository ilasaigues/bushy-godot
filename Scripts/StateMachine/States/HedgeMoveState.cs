
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeMoveState : BaseChildState<PlayerController, HedgeParentState>
    {

        Vector2 CurrentVelocity;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            ParentState.Direction = Agent.MovementInputVector;

            VelocityUpdate();
            prevStatus.AnimationLevel |= AnimationUpdate();
            return prevStatus;
        }

        protected void VelocityUpdate()
        {
            CurrentVelocity = new Vector2(
                (float)ParentState.xAxisMovement.Velocity,
                (float)ParentState.yAxisMovement.Velocity);

            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = CurrentVelocity;
        }
        private StateAnimationLevel AnimationUpdate()
        {
            Agent.Sprite2DComponent.ForceOrientation(CurrentVelocity);
            Agent.AnimController.SetBlendValue(PlayerController.AnimConditions.BushBlendValues, CurrentVelocity);
            return StateAnimationLevel.Uninterruptible;
        }

        protected override void ExitStateInternal()
        {
        }
    }
}
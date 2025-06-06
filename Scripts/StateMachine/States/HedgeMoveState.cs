
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeMoveState : BaseChildState<PlayerController, HedgeParentState>
    {
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            CheckHedge();
            VelocityUpdate();
            prevStatus.AnimationLevel |= AnimationUpdate();
            return prevStatus;
        }

        private void CheckHedge()
        {
            if (Agent.MovementComponent.HedgeState != HedgeOverlapState.Complete)
            {
                ChangeState<HedgeExitState>();
            }
        }

        protected void VelocityUpdate()
        {
            var xVel = (float)ParentState.xAxisMovement.Velocity;
            var yVel = (float)ParentState.yAxisMovement.Velocity;
            ParentState.SetVelocity(new Vector2(xVel, yVel));
            if (ParentState.CurrentVelocity.Length() > Agent.CharacterVariables.HedgeMovementSpeed)
            {
                ParentState.SetVelocity(ParentState.CurrentVelocity.Normalized() * Agent.CharacterVariables.HedgeMovementSpeed);
            }
        }
        private StateAnimationLevel AnimationUpdate()
        {
            Agent.Sprite2DComponent.ForceOrientation(ParentState.CurrentVelocity);
            Agent.AnimController.SetBlendValue(PlayerController.AnimConditions.BushBlendValues, ParentState.CurrentVelocity);
            return StateAnimationLevel.Uninterruptible;
        }

        protected override void ExitStateInternal()
        {
        }
    }
}
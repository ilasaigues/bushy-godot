using System;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class FallState : BaseChildState<PlayerController, AirParentState>
    {
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Falling, true);
        }

        protected override void ExitStateInternal()
        {
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Falling, false);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (prevStatus.CanChangeAnimation)
            {
                prevStatus.AnimationLevel |= UpdateAnimation();
            }
            if (prevStatus.StateExecutionResult != StateExecutionResult.Block)
            {
                ParentState.HandleGravity(delta);
            }
            return prevStatus;
        }

        public override StateAnimationLevel UpdateAnimation()
        {
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Falling, true);
            return StateAnimationLevel.Regular;
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction Action)
        {
            if (ParentState.CanCoyoteJump && actionType == InputAction.InputActionType.InputPressed && Action == InputManager.Instance.JumpAction)
            {
                throw StateInterrupt.New<JumpState>(false,
                    new StateConfig.InitialVelocityVectorConfig(Agent.MovementComponent.Velocities[VelocityType.MainMovement])
                );
            }
            return true;
        }


    }
}
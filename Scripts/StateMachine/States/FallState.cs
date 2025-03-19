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
        }

        protected override void ExitStateInternal()
        {
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
            Agent.AnimationComponent.Play("fall");
            return StateAnimationLevel.Regular;
        }

        public override bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            if (ParentState.CanCoyoteJump && actionType == InputAction.InputActionType.InputPressed && Action == InputManager.Instance.JumpAction)
            {
                throw new StateInterrupt<JumpState>(
                    new StateConfig.InitialVelocityVectorConfig(Agent.MovementComponent.Velocities[VelocityType.MainMovement])
                );
            }
            return true;
        }


    }
}
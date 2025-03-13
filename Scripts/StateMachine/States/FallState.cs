using System;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class FallState : BasePlayerState, IChildState<PlayerController, AirParentState>
    {
        public AirParentState ParentState { get; set; }

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
            return prevStatus;
        }

        public override void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            if (ParentState.CanCoyoteJump && actionType == InputAction.InputActionType.InputPressed && Action == InputManager.Instance.JumpAction)
            {
                throw new StateInterrupt<JumpState>();
            }
        }


        public override StateAnimationLevel UpdateAnimation()
        {
            if (Agent.MovementComponent.CurrentVelocity.Y > 0 && Agent.AnimationComponent.CurrentAnimation != "fall")
            {
                Agent.AnimationComponent.ClearQueue();
                Agent.AnimationComponent.Play("peak");
                Agent.AnimationComponent.Queue("fall");
            }
            return StateAnimationLevel.Regular;
        }
    }
}
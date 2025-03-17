using System;
using System.Diagnostics;
using System.Reflection;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class JumpState : BaseChildState<PlayerController, AirParentState>
    {
        [Export] public Timer DurationTimer;
        public bool JumpEnded = false;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            DurationTimer.WaitTime = Agent.CharacterVariables.JumpDuration;
            DurationTimer.Start();
            Agent.PlayerInfo.CanJump = false;
            ParentState.CanCoyoteJump = false;
            JumpEnded = false;
            UpdateAnimation();
        }

        protected override void ExitStateInternal()
        {
            JumpEnded = false;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (prevStatus.CanChangeAnimation)
            {
                prevStatus.AnimationLevel |= UpdateAnimation();
            }
            if (prevStatus.StateExecutionResult != StateExecutionResult.Block)
            {
                if (JumpEnded)
                {
                    ParentState.HandleGravity(delta);
                }
            }
            return new StateExecutionStatus(prevStatus);
        }

        public override bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            if (actionType == InputAction.InputActionType.InputReleased && Action == InputManager.Instance.JumpAction)
            {
                JumpActionEnded();
            }
            return true;
        }


        public void JumpActionEnded()
        {
            if (ParentState.VerticalVelocity < 0)
            {
                ParentState.VerticalVelocity = Agent.CharacterVariables.JumpShortHopSpeed;
                JumpEnded = true;
            }
        }

        private void JumpTimedOut()
        {
            if (ParentState.VerticalVelocity < 0)
            {
                JumpEnded = true;
            }
        }


        public override StateAnimationLevel UpdateAnimation()
        {
            Agent.AnimationComponent.ClearQueue();
            Agent.AnimationComponent.Play("jump");
            Agent.AnimationComponent.Queue("ascent");
            return StateAnimationLevel.Regular;
        }
    }
}
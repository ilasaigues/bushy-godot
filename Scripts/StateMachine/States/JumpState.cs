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
            ParentState.VerticalVelocity = Agent.CharacterVariables.JumpSpeed;
            JumpEnded = false;
            Agent.AnimController.SetTrigger(PlayerController.AnimConditions.JumpTrigger);
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
                if (ParentState.VerticalVelocity > Agent.CharacterVariables.AirSpeedThresholds.X)
                {
                    throw StateInterrupt.New<FallState>();
                }
            }
            return new StateExecutionStatus(prevStatus);
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction Action)
        {
            if (!JumpEnded && actionType == InputAction.InputActionType.InputReleased && Action == InputManager.Instance.JumpAction)
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
            return StateAnimationLevel.Regular;
        }
    }
}
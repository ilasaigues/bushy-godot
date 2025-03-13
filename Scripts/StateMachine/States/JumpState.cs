using System;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class JumpState : BasePlayerState, IChildState<PlayerController, AirParentState>
    {
        public override PlayerController Agent { get; set; }
        public AirParentState ParentState { get; set; }
        [Export] public Timer DurationTimer;
        public bool JumpEnded = false;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            DurationTimer.WaitTime = Agent.CharacterVariables.JumpDuration;
            DurationTimer.Start();
            DurationTimer.Timeout += JumpActionEnded;
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
            return new StateExecutionStatus(prevStatus);
        }

        public override void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            if (actionType == InputAction.InputActionType.InputReleased && Action == InputManager.Instance.JumpAction)
            {
                JumpActionEnded();
            }
        }


        public void JumpActionEnded()
        {
            if (!JumpEnded && ParentState.VerticalVelocity < 0)
            {
                ParentState.VerticalVelocity = Agent.CharacterVariables.JumpShortHopSpeed;
                JumpEnded = true;
                DurationTimer.Timeout -= JumpActionEnded;
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
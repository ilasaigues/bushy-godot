using Godot;
using GodotUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using static MovementComponent;

namespace BushyCore 
{
	[Scene]
	public partial class GroundedState : BaseMovementState
	{
		[Node]
		private Timer DashCooldownTimer;
		private double verticalVelocity;
		private bool canBufferJump;
		public override void _Ready()
		{
			base._Ready();

            this.AddToGroup();
            this.WireNodes();
		}

		protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			canBufferJump = true;
			SetCanDash();
			actionsComponent.CanJump = true;
			movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
			actionsComponent.JumpActionStart += JumpActionRequested;
			actionsComponent.DashActionStart += DashActionRequested;

			base.HorizontalAcceleration = characterVariables.GroundHorizontalAcceleration;
			base.HorizontalDeceleration = characterVariables.GroundHorizontalDeceleration;
			base.HorizontalMovementSpeed = characterVariables.GroundHorizontalMovementSpeed;
			base.HorizontalOvercappedDeceleration = characterVariables.GroundHorizontalOvercappedDeceleration;

			SetupFromConfigs(configs);
		}
		private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialGroundedConfig groundedConfig)
				{
					canBufferJump = groundedConfig.CanBufferJump;
				}
			}
		}

        public override void StateExit()
        {
            base.StateExit();

			actionsComponent.JumpActionStart -= JumpActionRequested;
			actionsComponent.DashActionStart -= DashActionRequested;
			actionsComponent.CanDash = true;
        }
        public override void StateUpdateInternal(double delta)
		{
			// Having a downwards velocity constantly helps snapping the character to the ground
			// We have to keep in mind that while using move and slide this WILL impact the characterś movement horizontally
			verticalVelocity = -10f;

			base.StateUpdateInternal(delta);
			HandleMovement(delta);
			CheckTransitions();
			VelocityUpdate();
		}

		void CheckTransitions()
		{
			if (canBufferJump && actionsComponent.IsJumpRequested && actionsComponent.CanJump)
			{	
				actionsComponent.Jump();
			}

			if (movementComponent.SnappedToFloor) return;
			
			movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			actionsComponent.Fall();
		}

		public void DashActionRequested()
		{
			if (actionsComponent.CanDash)
			{
				RunAndEndState(() => actionsComponent.Dash(this.IntendedDirection));
			}
		}

		public void JumpActionRequested()
		{
			if (actionsComponent.CanJump)
			{
				RunAndEndState(() => actionsComponent.Jump());
			}
		} 

		void HandleMovement(double _deltaTime)
		{
			if (actionsComponent.MovementDirection.X != 0)
				animationComponent.Play("run");
			else 
				animationComponent.Play("idle");
		}

		void SetCanDash()
		{
			var dashCdRemaining = this.characterVariables.DashCooldown - (Time.GetTicksMsec() - actionsComponent.LastDashTime);
			if (dashCdRemaining <= 0)
				actionsComponent.CanDash = true;
			else
				DashCooldownTimer.WaitTime = dashCdRemaining / 1000;

			if (!actionsComponent.CanDash)
				DashCooldownTimer.Start();
		}

        void DashCooldownTimerTimeout()
        {
			if (!this.IsActive) return;

			actionsComponent.CanDash = true;
        }

        protected override void VelocityUpdate()
        {
			var slopeVerticalComponent = Mathf.Tan(movementComponent.FloorAngle) * (float) horizontalVelocity;
			movementComponent.Velocities[VelocityType.Gravity] = movementComponent.FloorNormal * (float) verticalVelocity * 10;
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float)horizontalVelocity, slopeVerticalComponent);
        }
    }

}

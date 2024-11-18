using BushyCore;
using Godot;
using System;
using GodotUtilities;
using static MovementComponent;
using System.Diagnostics;

namespace BushyCore
{
	[Scene]
	public partial class DashState : BaseState
	{
		private Vector2 constantVelocity;
		private bool bufferJump;
		private float direction;
		private HedgeNode hedgeNode;

		[Node]
    	private Timer DurationTimer;
		[Node]
		private RayCast2D SlopeRaycast2D;

		private int state;
		public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

		protected override void AnimationUpdate() {}

		protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			SetupFromConfigs(configs);
			direction = actionsComponent.MovementDirection.X != 0 
				? Mathf.Sign(actionsComponent.MovementDirection.X)
				: Mathf.Sign(movementComponent.FacingDirection.X);

			movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			state = 0;
			actionsComponent.JumpActionStart += JumpActionRequested;
			actionsComponent.CanDash = false;
			bufferJump = false;
			
			DurationTimer.WaitTime = characterVariables.DashInitTime;
			DurationTimer.Start();
			movementComponent.CourseCorrectionEnabled = true;

			constantVelocity.X = characterVariables.DashVelocity;
			animationComponent.Play("dash_end");

            base.collisionComponent.CallDeferred(
                CharacterCollisionComponent.MethodName.SwitchShape, 
                (int) CharacterCollisionComponent.ShapeMode.CILINDER);
			
			collisionComponent.ToggleHedgeCollision(false);
				
		}


        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
				{
					constantVelocity = velocityConfig.Velocity.Normalized() * this.characterVariables.DashVelocity;
				}
			}
		}
		public override void StateExit()
        {
            base.StateExit();
			
			movementComponent.CourseCorrectionEnabled = false;
			actionsComponent.JumpActionStart -= JumpActionRequested;

			var exitVelocity = characterVariables.DashExitVelocity;
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2(
				exitVelocity * direction, 
				movementComponent.Velocities[VelocityType.MainMovement].Y);

			if (!movementComponent.IsInHedge)
				collisionComponent.ToggleHedgeCollision(true);
        }
    	public override void StateUpdateInternal(double delta)
    	{
			if (actionsComponent.IsJumpRequested)
				JumpActionRequested();

			VelocityUpdate();
			CheckHedge();
		}

		private void CheckHedge()
		{
			if (!movementComponent.IsInHedge) return;

			animationComponent.Play("turn");
			actionsComponent.EnterHedge(movementComponent.HedgeEntered, direction * Vector2.Right);
		}
    	protected override void VelocityUpdate() 
		{

			float verticalComponent = 0;

			// If grounded and non UP ground normal is detected, check whether this angle is for a slope or just a bump 
			if (movementComponent.SnappedToFloor && movementComponent.FloorAngle != 0)
			{
				var raycastDirection = SlopeRaycast2D.TargetPosition;
				SlopeRaycast2D.TargetPosition = raycastDirection * direction;
				
				SlopeRaycast2D.ForceRaycastUpdate();

				GodotObject collider = SlopeRaycast2D.GetCollider();

				if (collider != null && collider is TileMap && SlopeRaycast2D.GetCollisionNormal().X != 0) 
				{
					verticalComponent = Mathf.Tan(movementComponent.FloorAngle) * constantVelocity.X * direction;
				}

				SlopeRaycast2D.TargetPosition = raycastDirection;
			} 

			float horizontalComponent = constantVelocity.X * direction;
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2(horizontalComponent, verticalComponent);
		}

		public void JumpActionRequested()
		{
			if (actionsComponent.CanJump)
			{
				switch(state)
				{
					case 0:
						DurationTimer.Stop();
						movementComponent.Velocities[VelocityType.MainMovement] = new Vector2(characterVariables.DashJumpSpeed * direction,0);
						RunAndEndState(() => actionsComponent.Jump(this.characterVariables.DashJumpSpeed, false, true));
						break;
					case 2:
						bufferJump = true;
						break;
				}
			}
		} 
	
		void _on_duration_timer_timeout()
		{
			if (!IsActive) return;

			switch(state)
			{
				case 0:
					DurationTimer.Stop();
					DurationTimer.WaitTime = characterVariables.DashTime;
					DurationTimer.Start();
					break;
				case 1:
					DurationTimer.Stop();
					constantVelocity.X = characterVariables.DashExitVelocity;
					DurationTimer.WaitTime = characterVariables.DashExitTime;
					DurationTimer.Start();
					break;
				case 2:
					DurationTimer.Stop();
					RunAndEndState(() => {
						if (movementComponent.IsOnFloor)
						{
							animationComponent.Play("turn");
							actionsComponent.Land(StateConfig.InitialGroundedJumpBuffer(bufferJump));
						}
						actionsComponent.Fall();
					});
					break;
			}
			state ++;
		}
	}
}
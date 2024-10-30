using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
	[Scene]
	public partial class GroundedState : BaseState
	{
		[Node]
		private Timer DashCooldownTimer;
		private double verticalVelocity;
		private bool canBufferJump;

		private AxisMovement xAxisMovement;
		public override void _Ready()
		{
			base._Ready();

			this.AddToGroup();
			this.WireNodes();
		}

		public override void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationComponent anim, CharacterCollisionComponent col)
		{
			base.InitState(mc, cv, ac, anim, col);

			this.xAxisMovement = new AxisMovement.Builder()
				.Acc(characterVariables.GroundHorizontalAcceleration)
				.Dec(characterVariables.GroundHorizontalDeceleration)
				.Speed(characterVariables.GroundHorizontalMovementSpeed)
				.OverDec(characterVariables.GroundHorizontalOvercappedDeceleration)
				.TurnDec(characterVariables.HorizontalTurnDeceleration)
				.Movement(mc)
				.Direction(() => { return ac.MovementDirection.X; })
				.ColCheck((dir) => { return mc.IsOnWall; })
				.Variables(cv)
				.Build();
			
		}

		protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			canBufferJump = true;
			SetCanDash();
			
			xAxisMovement.SetInitVel(movementComponent.Velocities[VelocityType.MainMovement].X);

			actionsComponent.CanJump = true;
			movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;;
			actionsComponent.JumpActionStart += JumpActionRequested;
			actionsComponent.DashActionStart += DashActionRequested;

			xAxisMovement.OvershootDec(true);
			
			base.collisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);

			SetupFromConfigs(configs);
		}
		private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialGroundedConfig groundedConfig)
				{
					canBufferJump = groundedConfig.CanBufferJump;
					xAxisMovement.OvershootDec(groundedConfig.DoesDecelerate);
				}
			}
		}

		public override void StateExit()
		{
			base.StateExit();

			actionsComponent.JumpActionStart -= JumpActionRequested;
			actionsComponent.DashActionStart -= DashActionRequested;

			// Reconsider enabling this ALWAYS because we might have a really short dash CD if Bushy dashes close 
			// to the ground, lands, quickily jumps and redash
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

		protected override void AnimationUpdate()
		{
			float direction = actionsComponent.MovementDirection.X;

			if(direction != 0)
			{
				if(direction * xAxisMovement.Velocity < 0)
				{
					animationComponent.Play("turn");
					animationComponent.ClearQueue();
					animationComponent.Queue("run_start");
					animationComponent.Queue("run");
				}
				else if(animationComponent.CurrentAnimation != "run")
				{
					if (animationComponent.CurrentAnimation == "idle") animationComponent.Play("run_start");
					else  animationComponent.Queue("run_start");
					animationComponent.ClearQueue();
					animationComponent.Queue("run");
				}
				
			}
			else if(animationComponent.CurrentAnimation == "run")
			{
				animationComponent.Play("turn");
				animationComponent.ClearQueue();
				animationComponent.Queue("idle");
			}
			else
			{
				animationComponent.Queue("idle");
			}
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

		void HandleMovement(double deltaTime)
		{
			xAxisMovement.HandleMovement(deltaTime);
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
			var downwardVel = movementComponent.IsOnEdge ? 0 : 15;
			var slopeVerticalComponent = Mathf.Tan(movementComponent.FloorAngle) * (float) xAxisMovement.Velocity;
			movementComponent.Velocities[VelocityType.Gravity] = movementComponent.FloorAngle != 0 ? 
				movementComponent.FloorNormal *  (float) verticalVelocity * downwardVel
				: Vector2.Zero;
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float) xAxisMovement.Velocity, slopeVerticalComponent);
		}
	}

}

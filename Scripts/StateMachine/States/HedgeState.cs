using Godot;
using static MovementComponent;
using System;
using System.Security.AccessControl;
using System.Diagnostics;
using GodotUtilities;

namespace BushyCore 
{
    [Scene]
	public partial class HedgeState : BaseState
	{
		
		private Vector2 direction;
		private HedgeNode hedgeNode;
		private AxisMovement xAxisMovement;
		private AxisMovement yAxisMovement;

		[Node]
        private Timer EnteringTimer;
		[Node]
        private Timer JumpBufferTimer;
		[Node]
		private Timer DashBufferTimer;

		private int hedgePhase;
		private bool isExitJumpBuffer;
		private bool isExitDashBuffer;
		public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }
        public override void InitState(MovementComponent mc, 
			CharacterVariables cv,
			PlayerActionsComponent ac,
			AnimationComponent anim,
			CharacterCollisionComponent col,
			StateMachine sm)
        {
            base.InitState(mc, cv, ac, anim, col, sm);

			var builder = new AxisMovement.Builder()
				.Acc(characterVariables.HedgeAcceleration)
				.Dec(characterVariables.HedgeDeceleration)
				.Speed(characterVariables.HedgeMovementSpeed)
				.OverDec(characterVariables.HedgeOvercappedDeceleration)
				.TurnDec(characterVariables.HedgeTurnDeceleration)
				.HasOvershoot(true)
				.Movement(mc)
				.Direction(() => { return ac.MovementDirection.X; })
				.ColCheck((dir) => { return mc.IsOnWall; })
				.Variables(cv);

			xAxisMovement = builder.Build();
			yAxisMovement = builder.Copy()
				.Direction(() => { return ac.MovementDirection.Y; })
				.ColCheck((dir) => { return dir > 0 ? mc.IsOnFloor : mc.IsOnCeiling; })
				.ThresSpeed(characterVariables.MaxHedgeEnterSpeed)
				.Build();
        }

		private void RemoveControls() 
		{	
			EnteringTimer.Start();
			
			if (Mathf.Abs(xAxisMovement.Velocity) > characterVariables.HedgeMovementSpeed)
				xAxisMovement = xAxisMovement.ToBuilder().Copy()
					.Direction(() => 0)
					.Build();
			
			// The positive value is intentional. Only remove controls if the character is falling downwards
			if (yAxisMovement.Velocity > characterVariables.HedgeMovementSpeed)
				yAxisMovement = yAxisMovement.ToBuilder().Copy()
					.Direction(() => 0)
					.Build();
		}
		private void ReturnControls()
		{
			xAxisMovement = xAxisMovement.ToBuilder().Copy()
				.Direction(() => { return actionsComponent.MovementDirection.X; })
				.Build();
			yAxisMovement = yAxisMovement.ToBuilder().Copy()
				.Direction(() => { return actionsComponent.MovementDirection.Y; })
				.Build();
		}
        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			xAxisMovement.SetInitVel(movementComponent.CurrentVelocity.X);
			yAxisMovement.SetInitVel(movementComponent.CurrentVelocity.Y);

            movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			actionsComponent.CanJump = true;

			hedgePhase = 0;
			this.RemoveControls();

			base.collisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
			SetupFromConfigs(configs);

			hedgeNode.SubscribeMovementComponent(movementComponent);

			actionsComponent.JumpActionStart += OnJumpActionRequested;
			actionsComponent.JumpActionEnd += OnJumpActionCancelled;
			isExitJumpBuffer = false;

			actionsComponent.DashActionStart += OnDashActionRequested;
			actionsComponent.DashActionEnd += OnDashActionCancelled;
			isExitDashBuffer = false;
		}

		private void OnJumpActionRequested()
		{
			JumpBufferTimer.Start();
			isExitJumpBuffer = true;
		}

		private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialHedgeConfig hedgeConfig)
				{	
					direction = hedgeConfig.Direction.Normalized();
					hedgeNode = hedgeConfig.Hedge;
				}
			}
		}

        public override void StateExit()
		{
			collisionComponent.ToggleHedgeCollision(true);
			actionsComponent.CanDash = true;
			hedgeNode.UnSubscribeMovementComponent(movementComponent);
			base.StateExit();

			actionsComponent.JumpActionStart -= OnJumpActionRequested;
			actionsComponent.JumpActionEnd -= OnJumpActionCancelled;

			actionsComponent.DashActionStart -= OnDashActionRequested;
			actionsComponent.DashActionEnd -= OnDashActionCancelled;
		}

		public override void StateUpdateInternal(double delta)
        {
			direction = actionsComponent.MovementDirection;
            base.StateUpdateInternal(delta);

			yAxisMovement.HandleMovement(delta);
			xAxisMovement.HandleMovement(delta);
			
			VelocityUpdate();
        }

        protected override void VelocityUpdate()
        {
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float) xAxisMovement.Velocity, (float)yAxisMovement.Velocity);
		}

        protected override void AnimationUpdate()
        {

        }

		public void OnHedgeExit(HedgeNode hedgeNode)
		{
			if (!IsActive)
				return;
			
			RunAndEndState(() => {
				collisionComponent.ToggleHedgeCollision(true);

				var direction = movementComponent.FacingDirection;
				if (isExitDashBuffer && isExitJumpBuffer)
				{
					movementComponent.Velocities[VelocityType.MainMovement] = new Vector2(this.characterVariables.DashJumpSpeed * Mathf.Sign(direction.X),0);
					actionsComponent.CanDash = false;
					actionsComponent.Jump(this.stateMachine, this.characterVariables.DashJumpSpeed, false, true);
				}
					
				if (isExitDashBuffer)
				{
					actionsComponent.CanDash = false;
					actionsComponent.Dash(this.stateMachine, direction);
				}
					
				if (isExitJumpBuffer) 
					actionsComponent.Jump(this.stateMachine);
				
				actionsComponent.Fall(this.stateMachine);
			});
		}


        void EnteringTimerTimeout()
        {
            if (!this.IsActive) return;
			this.hedgePhase = 1;
			this.ReturnControls();
		}

		private void OnJumpActionCancelled()
		{
			JumpBufferTimer.Stop();
			// Consider adding a buffer to this input cancel
			isExitJumpBuffer = false;
		}
		private void OnJumpBufferTimerEnd()
		{
			if (IsActive)
				isExitJumpBuffer = false;
		}

		private void OnDashActionRequested()
		{
			DashBufferTimer.Start();
			isExitDashBuffer = true;
		}

		private void OnDashActionCancelled()
		{
			DashBufferTimer.Stop();
			// Consider adding a buffer to this input cancel
			isExitDashBuffer = false;
		}
		private void OnDashBufferTimerEnd()
		{
			if (IsActive)
				isExitDashBuffer = false;
		}

    }
}


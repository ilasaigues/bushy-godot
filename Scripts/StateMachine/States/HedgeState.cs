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

		private int hedgePhase;
		public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }
        public override void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationPlayer anim, CharacterCollisionComponent col)
        {
            base.InitState(mc, cv, ac, anim, col);

			var builder = new AxisMovement.Builder()
				.Acc(characterVariables.HedgeAcceleration)
				.Dec(characterVariables.HedgeDeceleration)
				.Speed(characterVariables.HedgeMovementSpeed)
				.OverDec(characterVariables.HedgeOvercappedDeceleration)
				.TurnDec(characterVariables.HedgeTurnDeceleration)
				.HasOvershoot(true)
				.Movement(mc)
				.Direction(() => { return ac.MovementDirection.X; })
				.Variables(cv);

			xAxisMovement = builder.Build();
			yAxisMovement = builder.Copy()
				.Direction(() => { return ac.MovementDirection.Y; })
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
			
			hedgePhase = 0;
			this.RemoveControls();

			base.collisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
			SetupFromConfigs(configs);
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
			
			base.StateExit();
		}

		public override void StateUpdateInternal(double delta)
        {
			direction = actionsComponent.MovementDirection;
            base.StateUpdateInternal(delta);

			xAxisMovement.HandleMovement(delta);
			yAxisMovement.HandleMovement(delta);
			VelocityUpdate();
        }

        protected override void VelocityUpdate()
        {
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float) xAxisMovement.Velocity, (float)yAxisMovement.Velocity);
			// Debug.WriteLine($"Hedge VEL: {movementComponent.Velocities[VelocityType.MainMovement]}");
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
				// TODO: Check if this fucks up when walking through a hedge at floor level
				actionsComponent.Fall();
			});
		}


        void EnteringTimerTimeout()
        {
            if (!this.IsActive) return;
			this.hedgePhase = 1;
			this.ReturnControls();
		}
    }
}


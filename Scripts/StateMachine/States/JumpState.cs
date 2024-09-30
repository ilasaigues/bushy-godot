using System;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    [Scene]
    public partial class JumpState : BaseState
    {
		private double verticalVelocity;
        private bool earlyDrop;
        private int targetVelocity;
		private AxisMovement xAxisMovement;
        private bool CanJumpIntoHedge;
        [Node]
        private Timer DurationTimer;
        private HedgeNode hedgeNodeEntered;

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

			this.xAxisMovement = new AxisMovement.Builder()
				.Acc(characterVariables.AirHorizontalAcceleration)
				.Dec(characterVariables.AirHorizontalDeceleration)
				.Speed(characterVariables.AirHorizontalMovementSpeed)
				.OverDec(characterVariables.AirHorizontalOvercappedDeceleration)
				.TurnDec(characterVariables.HorizontalTurnDeceleration)
				.Movement(mc)
				.Direction(() => { return ac.MovementDirection.X; })
				.Variables(cv)
				.Build();
        }

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
            animationComponent.ClearQueue();
            animationComponent.Play("jump");
            animationComponent.Queue("ascent");

            targetVelocity = characterVariables.AirHorizontalMovementSpeed;
            verticalVelocity = characterVariables.JumpSpeed;
            CanJumpIntoHedge = false;

            earlyDrop = false;

            xAxisMovement.OvershootDec(true);
            xAxisMovement.SetInitVel(movementComponent.Velocities[VelocityType.MainMovement].X);
            
            DurationTimer.WaitTime = characterVariables.JumpDuration;
            DurationTimer.Start();

            actionsComponent.CanJump = false;

			actionsComponent.DashActionStart += DashActionRequested;
			actionsComponent.JumpActionEnd += JumpActionEnded;

            base.collisionComponent.CallDeferred(
                CharacterCollisionComponent.MethodName.SwitchShape, 
                (int) CharacterCollisionComponent.ShapeMode.CILINDER);

            foreach (var config in configs)
            {
                if(config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    targetVelocity = (int) MathF.Abs(velocityConfig.Velocity.X);
                    xAxisMovement.OvershootDec(velocityConfig.DoesDecelerate);
                    CanJumpIntoHedge = velocityConfig.CanEnterHedge;
                }
            }

            if (CanJumpIntoHedge)
                collisionComponent.ToggleHedgeCollision(false);
		}

        protected override void AnimationUpdate() {}
        public override void StateExit()
        {
			actionsComponent.DashActionStart -= DashActionRequested;
			actionsComponent.JumpActionEnd -= JumpActionEnded;
            if (!movementComponent.IsInHedge)
                collisionComponent.ToggleHedgeCollision(true);
            base.StateExit();
        }
        public override void StateUpdateInternal(double delta)
        {
            base.StateUpdateInternal(delta);
            xAxisMovement.HandleMovement(delta);
            this.VelocityUpdate();
            // CheckSwing();

            CheckTransitions();
        }
        protected override void VelocityUpdate()
        {
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) xAxisMovement.Velocity * Vector2.Right;
        }

        void CheckTransitions()
        {
            if (CanJumpIntoHedge && movementComponent.IsInHedge)
                actionsComponent.EnterHedge(movementComponent.HedgeEntered, (float) xAxisMovement.Velocity * Vector2.Right);
            
            if (actionsComponent.IsJumpCancelled)
                actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpShortHopSpeed), xAxisMovement.HasOvershootDeceleration, CanJumpIntoHedge);

            if (movementComponent.IsOnCeiling)
                actionsComponent.Fall();
        }

        void DurationTimerTimeout()
        {
            if (!this.IsActive) return;
            RunAndEndState(() => actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpSpeed), xAxisMovement.HasOvershootDeceleration, CanJumpIntoHedge));
        }

		public void DashActionRequested()
		{
			if (actionsComponent.CanDash)
			{
				RunAndEndState(() => actionsComponent.Dash(this.IntendedDirection));
			}
		} 

        public void JumpActionEnded()
        {
            RunAndEndState(() => actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpShortHopSpeed), canFallIntoHedge: CanJumpIntoHedge));
        }
    }
}


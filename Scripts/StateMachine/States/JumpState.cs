using System;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    [Scene]
    public partial class JumpState : BaseMovementState
    {
		private double verticalVelocity;
        private bool earlyDrop;
        private int targetVelocity;

        [Node]
        private Timer DurationTimer;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

	    protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
            animationComponent.Play("jump");

            targetVelocity = characterVariables.AirHorizontalMovementSpeed;
            horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
            verticalVelocity = characterVariables.JumpSpeed;

            earlyDrop = false;

            DurationTimer.WaitTime = characterVariables.JumpDuration;
            DurationTimer.Start();

            actionsComponent.CanJump = false;

			actionsComponent.DashActionStart += DashActionRequested;
			actionsComponent.JumpActionEnd += JumpActionEnded;

            foreach (var config in configs)
            {
                if(config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    targetVelocity = (int) MathF.Abs(velocityConfig.Velocity.X);
                }
            }

			base.HorizontalAcceleration = characterVariables.AirHorizontalAcceleration;
			base.HorizontalDeceleration = characterVariables.AirHorizontalDeceleration;
			base.HorizontalOvercappedDeceleration = characterVariables.AirHorizontalOvercappedDeceleration;
			base.HorizontalMovementSpeed = targetVelocity;
		}
        public override void StateExit()
        {
			actionsComponent.DashActionStart -= DashActionRequested;
			actionsComponent.JumpActionEnd -= JumpActionEnded;
            base.StateExit();
        }
        public override void StateUpdateInternal(double delta)
        {
            base.StateUpdateInternal(delta);
            this.VelocityUpdate();
            // CheckSwing();

            CheckTransitions();
        }
        protected override void VelocityUpdate()
        {
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) horizontalVelocity * Vector2.Right;
        }

        void CheckTransitions()
        {
            if (actionsComponent.IsJumpCancelled)
                actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpShortHopSpeed));

            if (actionsComponent.IsDashRequested && actionsComponent.CanDash)
                actionsComponent.Dash(this.IntendedDirection);
        }

        void DurationTimerTimeout()
        {
            if (!this.IsActive) return;
            RunAndEndState(() => actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpSpeed)));
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
            RunAndEndState(() => actionsComponent.Fall(new Vector2(targetVelocity, characterVariables.JumpShortHopSpeed)));
        }
    }
}
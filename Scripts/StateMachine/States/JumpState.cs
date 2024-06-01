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
        private double horizontalVelocity;
		private double verticalVelocity;
        private bool earlyDrop;
        private float targetVelocity;

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
            // _directionInputSubscription = _input.DirectionInput.Subscribe(UpdateFacingDirection);
            animationComponent.Play("jump");
            targetVelocity = characterVariables.AirHorizontalMovementSpeed;
            horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
            verticalVelocity = characterVariables.JumpSpeed;

            earlyDrop = false;

            DurationTimer.WaitTime = characterVariables.JumpDuration;
            DurationTimer.Start();

            actionsComponent.CanJump = false;

			actionsComponent.DashActionStart += DashActionRequested;

            foreach (var config in configs)
            {
                if(config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    targetVelocity = MathF.Abs(velocityConfig.Velocity.X);
                }
            }
		}
        public override void StateExit()
        {
			actionsComponent.DashActionStart -= DashActionRequested;
            base.StateExit();
            // _directionInputSubscription?.Dispose();
        }
        public override void StateUpdateInternal(double delta)
        {
            HandleHorizontalMovement(delta);
            this.VelocityUpdate();
            // CheckDash();
            // CheckSwing();

            CheckTransitions();
        }
        protected override void VelocityUpdate()
        {
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) horizontalVelocity * Vector2.Right;
        }

        void HandleHorizontalMovement(double deltaTime)
        {

            Vector2 direction = actionsComponent.MovementDirection;
            if (direction.X != 0) // If we're doing any input
            {
                //if the input direction is opposite of the current direction, we also add a deceleration
                if (direction.X * horizontalVelocity < 0)
                {
                    horizontalVelocity += direction.X * characterVariables.AirHorizontalDeceleration * deltaTime;
                }

                if (Mathf.Abs(horizontalVelocity) <= Mathf.Abs(targetVelocity))
                {
                    horizontalVelocity += direction.X * characterVariables.AirHorizontalAcceleration * deltaTime;
                    horizontalVelocity = Mathf.Clamp(horizontalVelocity, -targetVelocity, targetVelocity);
                }
                else
                {
                    horizontalVelocity += characterVariables.AirHorizontalOvercappedDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                }

            }
            else //if we're not doing any input, we decelerate to 0
            {
                var deceleration = characterVariables.AirHorizontalDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                if (Mathf.Abs(deceleration) < Mathf.Abs(horizontalVelocity))
                {
                    horizontalVelocity += deceleration;
                }
                else
                {
                    horizontalVelocity = 0;
                }
            }
        }

        void CheckTransitions()
        {
            earlyDrop |= !actionsComponent.IsJumpRequested;
            
            if (earlyDrop)
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
    }
}
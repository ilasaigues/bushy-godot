using Godot;
using System;
using System.Diagnostics;
using static MovementComponent;

namespace BushyCore 
{
	public partial class GroundedState : BaseState
	{
        private double horizontalVelocity;
        private double verticalVelocity;

        public override void StateEnter(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnter(configs);

            actionsComponent.CanJump = actionsComponent.CanDash = true;
            movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;

            horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
        }

		public override void StateUpdateInternal(double delta)
		{
			verticalVelocity = 0f;

            HandleMovement(delta);
            
			var floorNormal = movementComponent.FloorNormal;
			movementComponent.Velocities[VelocityType.Gravity] = floorNormal * (float) verticalVelocity * 10;
            movementComponent.Velocities[VelocityType.MainMovement] = floorNormal.Orthogonal().Normalized() * (float) -horizontalVelocity;
		}

		void CheckTransitions()
        {
			if (actionsComponent.IsJumpRequested)
            {
                actionsComponent.Jump();
            }

            bool stillGrounded = movementComponent.IsOnFloor;

            if (stillGrounded) return;
            
			verticalVelocity = 0;
			movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			actionsComponent.Fall();
        }

		void HandleMovement(double deltaTime)
        {
			Vector2 direction = actionsComponent.MovementDirection;
			var vars = characterVariables;
            if (direction.X != 0)
            {
                horizontalVelocity += direction.X * vars.GroundHorizontalAcceleration * deltaTime;
                //if the input direction is opposite of the current direction, we also add a deceleration
                if (direction.X * horizontalVelocity < 0)
                {
                    horizontalVelocity += direction.X * vars.GroundHorizontalDeceleration * deltaTime;
                }
            }
            else //if we're not doing any input, we decelerate to 0
            {
                var deceleration = vars.GroundHorizontalDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                if (Mathf.Abs(deceleration) < Mathf.Abs(horizontalVelocity))
                {
                    horizontalVelocity += deceleration;
                }
                else
                {
                    horizontalVelocity = 0;
                }
            }
            horizontalVelocity = Mathf.Clamp(horizontalVelocity, -vars.GroundHorizontalMovementSpeed, vars.GroundHorizontalMovementSpeed);
        }
	}

}

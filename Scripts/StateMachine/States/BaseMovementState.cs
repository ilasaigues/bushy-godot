using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    public partial class BaseMovementState : BaseState
    {
        protected double horizontalVelocity;
        protected int HorizontalAcceleration;
        protected int HorizontalDeceleration;
        protected int HorizontalMovementSpeed;
        protected float HorizontalOvercappedDeceleration;
        protected bool HasOvershootDeceleration;
        public override void StateUpdateInternal(double delta)
        {
            HandleHorizontalMovement((float )delta);
        }

        protected override void VelocityUpdate() {}

        protected void HandleHorizontalMovement(float deltaTime)
        {
			Vector2 direction = actionsComponent.MovementDirection;
			var vars = characterVariables;
            
			if (direction.X != 0)
			{
                var targetVelocity = movementComponent.IsOnWall ? vars.MaxOnWallHorizontalMovementSpeed : HorizontalMovementSpeed;
				//if the input direction is opposite of the current direction, we also add a deceleration
                if (direction.X * horizontalVelocity < 0)
				{
                    horizontalVelocity += direction.X * vars.HorizontalTurnDeceleration * deltaTime;
				}
                else if (Mathf.Abs(horizontalVelocity) < Mathf.Abs(targetVelocity))
                {
                    horizontalVelocity += direction.X * HorizontalAcceleration * deltaTime;
                    horizontalVelocity = Mathf.Clamp(horizontalVelocity, -targetVelocity, targetVelocity);
                }
                else if (HasOvershootDeceleration || movementComponent.IsOnWall)
                {
                    horizontalVelocity += HorizontalOvercappedDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                    horizontalVelocity = Mathf.Max(targetVelocity, Mathf.Abs(horizontalVelocity)) * Mathf.Sign(movementComponent.FacingDirection.X);
                }
			}
			else //if we're not doing any input, we decelerate to 0
			{
				var deceleration = HorizontalDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
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
    }
}
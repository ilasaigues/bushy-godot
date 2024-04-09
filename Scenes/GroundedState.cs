using Godot;
using System;
using System.Diagnostics;

public partial class GroundedState : BaseState
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	public const float Gravity = 1000f;

    public override void UpdateInternal(double delta)
    {
		Vector2 velocity = movementComponent.CurrentVelocity;

		bool onFloor = movementComponent.IsOnFloor;
		// Add the gravity.
		if (!onFloor)
			velocity.Y += Gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && onFloor)
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(movementComponent.CurrentVelocity.X, 0, Speed);
		}

		
		movementComponent.Velocities[MovementComponent.VelocityType.MainMovement] = velocity;
    }

}

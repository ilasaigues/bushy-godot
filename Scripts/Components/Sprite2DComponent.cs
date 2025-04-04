using Godot;
using System;

public partial class Sprite2DComponent : Sprite2D
{
	[Export]
	private MovementComponent movementComponent;

	public override void _Ready()
	{
		//movementComponent.CoreographyUpdate += ForceOrientation;
	}
	public override void _ExitTree()
	{
		//movementComponent.CoreographyUpdate -= ForceOrientation;
	}
	public override void _Process(double delta)
	{
		CheckSpriteOrientation();
	}

	private void CheckSpriteOrientation()
	{
		var mainVelocity = movementComponent.Velocities[MovementComponent.VelocityType.MainMovement];

		if (mainVelocity != Vector2.Zero)
		{
			this.FlipH = movementComponent.FacingDirection.X < 0f;
			return;
		}
	}

	public void ForceOrientation(Vector2 direction)
	{
		this.FlipH = direction.X < 0f || (direction.X <= 0f && FlipH);
		this.FlipV = direction.Y > 0f || (direction.Y >= 0f && FlipV);
	}
}

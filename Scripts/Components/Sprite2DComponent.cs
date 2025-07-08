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
		FlipH = movementComponent.ParentController.PlayerInfo.LookDirection < 0;
	}

	public void ForceOrientation(Vector2 direction)
	{
		FlipH = direction.X < 0f || (direction.X <= 0f && FlipH);
		FlipV = direction.Y > 0f || (direction.Y >= 0f && FlipV);
	}
}

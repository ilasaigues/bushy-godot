using Godot;
using System;

public partial class Sprite2DComponent : Sprite2D
{
	[Export]
	private MovementComponent movementComponent;
	
	public override void _Process(double delta)
	{
		this.FlipH = movementComponent.FacingDirection.X < 0f;
	}
}

using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	private MovementComponent movementComponent;

	public override void _Ready()
	{
		this.movementComponent = GetNode<MovementComponent>("MovementComponent");
	}

	public override void _PhysicsProcess(double delta)
	{
		movementComponent.IsOnFloor = IsOnFloor();
		movementComponent.Move(this);
	}
}

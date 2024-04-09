using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class MovementComponent : Node2D
{
	public bool IsOnFloor; 
	
	public enum VelocityType
	{
		MainMovement,
		Gravity,
		InheritedVelocity,
		Dash,
	}
	public Dictionary<VelocityType, Vector2> Velocities = new Dictionary<VelocityType, Vector2>();
	public Vector2 CurrentVelocity => Velocities.Values.AsEnumerable()
		.Append(Vector2.Zero)
		.Aggregate((v1, v2) => v1 + v2);

	
	public void Move(CharacterBody2D characterBody2D)
	{
		characterBody2D.Velocity = CurrentVelocity;
		characterBody2D.MoveAndSlide();
	}

}

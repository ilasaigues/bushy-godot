using Godot;
using System;

public partial class camera_placeholder : Camera2D
{
	
	[Export]
	Camera2D cam;
	[Export]
	CharacterBody2D target;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float x = target.Position.X;
		float y = target.IsOnFloor() ? target.Position.Y : cam.Position.Y;

		cam.Position = new Vector2(x,y);


	}
}

using Godot;
using System;
using System.Diagnostics;
using System.Drawing;

public partial class camera_placeholder : Camera2D
{
	[Export]
	private CharacterBody2D target;
	[Export]
	float speed;
	
	[Export]
	Vector2 deadZone;

	Vector2 direction = Vector2.Zero;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float xDiff = target.GlobalPosition.X - GlobalPosition.X;
		float yDiff = target.GlobalPosition.Y - GlobalPosition.Y;
		if (MathF.Abs(xDiff) > deadZone.X/2)
		{
			GlobalPosition = new Vector2(target.GlobalPosition.X - deadZone.X/2 * MathF.Sign(xDiff), GlobalPosition.Y);
		}

		if (MathF.Abs(yDiff) > deadZone.Y/2)
		{
			GlobalPosition = new Vector2(GlobalPosition.X, + target.GlobalPosition.Y - deadZone.Y/2 * MathF.Sign(yDiff));
		}

		if(target.IsOnFloor())
		{
			GlobalPosition = new Vector2(GlobalPosition.X, target.GlobalPosition.Y);
		}

	}

	public override void _Draw() {
		base._Draw();
		Vector2 size = new Vector2(deadZone.X,deadZone.Y);
		Rect2 deadzoneRect = new Rect2(-size/2, size);

		DrawRect(deadzoneRect, Godot.Color.Color8(128,128,128,50));
	}
	
}

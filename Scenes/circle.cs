using Godot;
using System;

public partial class circle : ColorRect
{
	float counter;
	[Export]
	float offset;
	Vector2 desired_position;
	Vector2 initial_position;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		initial_position = Position;
		counter = offset;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		counter += (float)delta;
		desired_position = Position + new Vector2(MathF.Sin(counter), Mathf.Cos(counter));
		desired_position += new Vector2(MathF.Sin(counter/2), 0);
		
	}


	void _on_timer_timeout()
	{
		Position = desired_position;
	}
}

using Godot;
using System;

public partial class Injector : Node2D
{
	[Export]
	VisualComponent visuals;
	[Export]
	Animation[] animations;
	double timer;
	public override void _Ready()
	{
		visuals.Init();
		visuals.PlayAnimation(animations[0], true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += delta;
		if(timer > 4)
		{
			visuals.PlayAnimation(animations[1],true);
			visuals.PlayAnimation(animations[0],false);
			timer = 0;
		} 

	}
}

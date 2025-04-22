using Godot;
using System;
using System.Diagnostics;

public partial class LineTrail : Line2D
{
	private Curve2D curve;
	private Node2D parent;
	[Export]
	private int maxPoints;

	public override void _Ready()
	{
		curve = new Curve2D();
		parent = GetParent<Node2D>();
	}

	public override void _Process(double delta)
	{
		curve.AddPoint(parent.GlobalPosition);
		if (curve.PointCount > maxPoints)
			curve.RemovePoint(0);
		Points = curve.GetBakedPoints();
	}

	public async void Stop()
	{
		// this.SetProcess(false);
		// Tween tw = GetTree().CreateTween();
		// tw.TweenProperty(this, "modulate:a", 0.0, 3.0);
		// await tw.Finished();

	}
}

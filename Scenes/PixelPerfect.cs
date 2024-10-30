using Godot;
using System;

public partial class PixelPerfect : CanvasLayer
{
	[Export]
	Camera2D mainCamera;
	[Export]
	Camera2D pixelPefectCamera;
	[Export]
	SubViewport viewport;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Collections.Array<Node> PixelPerfectStuff = GetTree().GetNodesInGroup("pixel_perfect");
		GD.Print(PixelPerfectStuff.Count);
		foreach (Node item in PixelPerfectStuff)
		{
			item.CallDeferred("reparent", viewport, true);
		}
	}

	public override void _Process(double delta)
	{
		pixelPefectCamera.GlobalTransform = mainCamera.GlobalTransform;
	}
}

using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class HitboxComponent : Area2D
{
	public CollisionShape2D collisionShape2D;
	[Export]
	public bool IsDebug;
	[Export]
	public Color DebugColor;
	
	public override void _Ready()
	{
		collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
	}

    public override void _Draw()
    {
		if (Engine.IsEditorHint())
                return;

		if (IsDebug && !collisionShape2D.Disabled && collisionShape2D.Shape != null)
		{	
			DrawRect(collisionShape2D.Shape.GetRect(), DebugColor);
		}
    }

    public void ToggleEnable(bool enabled) 
	{
		collisionShape2D.Disabled = !enabled;
		QueueRedraw();
	}
}

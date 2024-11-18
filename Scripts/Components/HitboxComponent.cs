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

	[Signal]
	public delegate void HitboxImpactEventHandler();
	
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

	public void OnHitboxImpact(Area2D area2D)
	{
		// Make sure this is an enemy then emit event
		// Obtain hurtbox component from the area ipmacted and provide it to the CombatState Machine?
		// What amount of damage does this inflict?
		EmitSignal(SignalName.HitboxImpact);
	}
}

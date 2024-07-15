using Godot;
using System;
using GodotUtilities;
using BushyCore;

[Scene]
public partial class PlayerController : CharacterBody2D
{
	[Node]
	private MovementComponent MovementComponent;

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			this.AddToGroup();
			this.WireNodes();
		}
	}

    public override void _Ready()
    {
        base._Ready();
    }
    public override void _PhysicsProcess(double delta)
	{
		MovementComponent.UpdateState(this);
		MovementComponent.Move(this);
	}
}

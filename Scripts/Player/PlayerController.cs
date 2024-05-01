using Godot;
using System;
using GodotUtilities;

[Scene]
public partial class PlayerController : CharacterBody2D
{
	[Node]
	private MovementComponent MovementComponent;
	[Node]
	private CollisionShape2D CollisionShape2D;	

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

		MovementComponent.SetRaycastPosition(CollisionShape2D);
    }
    public override void _PhysicsProcess(double delta)
	{
		MovementComponent.UpdateState(this);
		MovementComponent.Move(this);
	}
}

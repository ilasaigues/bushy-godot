using BushyCore;
using Godot;
using GodotUtilities;
using System.Diagnostics;

[Scene]
public partial class HedgeStaticBody2D : AnimatableBody2D
{
	[Export]
	public HedgeNode HedgeNode {get; private set;}
	[Node]
	public ColorRect ColorRect;
	[Node]
	public CollisionShape2D CollisionShape2D;

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			this.AddToGroup();
			this.WireNodes();
		}
	}
}

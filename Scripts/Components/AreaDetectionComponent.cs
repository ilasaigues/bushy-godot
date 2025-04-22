using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using GodotUtilities;


namespace BushyCore
{
	[Scene]
	public partial class AreaDetectionComponent : Area2D
	{
		[Signal]
		public delegate void OnAreaEnterEventHandler(Area2D areaNode);
		[Signal]
		public delegate void OnAreaExitEventHandler(Area2D areaNode);

		[Node]
		private CollisionShape2D CollisionShape2D;

		public override void _Notification(int what)
		{
			if (what == NotificationSceneInstantiated)
			{
				this.AddToGroup();
				WireNodes();
			}
		}

		public void OnAreaExited(Area2D area2D)
		{
			EmitSignal(SignalName.OnAreaExit, area2D);
		}

		public void OnAreaEntered(Area2D area2D)
		{
			EmitSignal(SignalName.OnAreaEnter, area2D);
		}
	}
}
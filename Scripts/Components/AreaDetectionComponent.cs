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
		public delegate void OnHedgeEnterEventHandler(HedgeNode hedgeNode);
		[Signal]
		public delegate void OnHedgeExitEventHandler(HedgeNode hedgeNode);

		[Node]
		private CollisionShape2D CollisionShape2D;

		HashSet<HedgeArea2D> _overlappingHedges = new();

		public override void _Notification(int what)
		{
			if (what == NotificationSceneInstantiated)
			{
				this.AddToGroup();
				this.WireNodes();
			}
		}

		public void OnAreaExited(Area2D area2D)
		{
			if (area2D is HedgeArea2D hedgeArea2D)
			{
				_overlappingHedges.Remove(hedgeArea2D);
				if (_overlappingHedges.Count == 0)
				{
					EmitSignal(SignalName.OnHedgeExit, hedgeArea2D.HedgeNode);
				}
			}
		}

		public void OnAreaEntered(Area2D area2D)
		{
			if (area2D is HedgeArea2D hedgeArea2D)
			{
				_overlappingHedges.Add(hedgeArea2D);
				EmitSignal(SignalName.OnHedgeEnter, hedgeArea2D.HedgeNode);
			}
		}
	}
}
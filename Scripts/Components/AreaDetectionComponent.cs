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
			if (area2D.GetParent() is HedgeNode hedgeNode)
			{
				EmitSignal(SignalName.OnHedgeExit, hedgeNode);
			}
		}

		public void OnAreaEntered(Area2D area2D)
		{
			if (area2D.GetParent() is HedgeNode hedgeNode)
			{
				EmitSignal(SignalName.OnHedgeEnter, hedgeNode);
			}
		}
	}
}
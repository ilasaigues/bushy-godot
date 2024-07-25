using Godot;
using GodotUtilities;
using System;

namespace BushyCore
{	
	[Scene] 
	public partial class HedgeNode : Node2D
	{
		[Node]
		private StaticBody2D StaticBody2D;
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void ToggleHedgeCollision(bool isOn)
		{
			StaticBody2D.SetCollisionLayerValue(3, isOn);
			// StaticBody2D.CallDeferred(StaticBody2D.MethodName.SetCollisionLayerValue, 3, isOn);
		}

		public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }
	}
}

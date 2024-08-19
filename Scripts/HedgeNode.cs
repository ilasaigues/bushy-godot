using Godot;
using GodotUtilities;
using System;
using System.Diagnostics;

namespace BushyCore
{	
	[Scene] 
	public partial class HedgeNode : Node2D
	{
		[Node]
		private StaticBody2D StaticBody2D;
		[Node]
		private Area2D HedgeArea2D;


		public void ToggleHedgeCollision(bool isOn)
		{
			StaticBody2D.SetCollisionLayerValue(3, isOn);
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

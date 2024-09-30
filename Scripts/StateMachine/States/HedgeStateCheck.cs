using Godot;
using GodotUtilities;
using System;
using System.Diagnostics;

namespace BushyCore 
{
	[Scene]
	public partial class HedgeStateCheck : Node2D
	{
		// For now we're pretty stupidly using a single raycast for hedge checks. Will have to make it more detailed later
		[Node]
		private RayCast2D HedgeRaycastTop;
		[Node]
		private RayCast2D HedgeRaycastBot;
		
		[Signal]
		public delegate void OnHedgeEnterEventHandler(HedgeNode hedgeNode);

		private HedgeNode hedgeNode;
		private Vector2 checkerDirection;
		private MovementComponent movementComponent;
		public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

		public void CheckerInit(MovementComponent movementComponent)
		{
			this.movementComponent = movementComponent;
			HedgeRaycastBot.Enabled = false;
		}
		public void CheckerReset(Vector2 checkerDirection)
		{
			this.hedgeNode = null;
			this.checkerDirection = checkerDirection; 
		}
		
		public HedgeNode CheckHedgeCollision()
		{
			if (hedgeNode != null)
				return hedgeNode;

			if (!movementComponent.IsOnWall && !movementComponent.IsOnCeiling)
				return null;

			HedgeRaycastBot.Position = Vector2.Zero;
			HedgeRaycastBot.TargetPosition = Mathf.Sign(checkerDirection.X) * Vector2.Right * 12;

			HedgeRaycastBot.ForceRaycastUpdate();
			var hedgeCollider = HedgeRaycastBot.GetCollider();
			
			if (hedgeCollider == null) 
				return null;

			if (hedgeCollider is StaticBody2D sb && sb.GetParent() is HedgeNode hedgeCollision)
			{
				hedgeNode = hedgeCollision;
				EmitSignal(SignalName.OnHedgeEnter, hedgeNode);
				return hedgeNode;
			}
			return null;
		}
	}
}


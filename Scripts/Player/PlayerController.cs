using Godot;
using System;
using GodotUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BushyCore
{
	[Scene]
	public partial class PlayerController : CharacterBody2D
	{
		[Node]
		public MovementComponent MovementComponent;
		[Node]
		private CharacterCollisionComponent CollisionComponent;

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
			MovementComponent.SetParentController(this);
			CollisionComponent.SetParentController(this);
		}

		public override void _PhysicsProcess(double delta)
		{
			MovementComponent.UpdateState(this);
			MovementComponent.Move(this);
		}
	}

}

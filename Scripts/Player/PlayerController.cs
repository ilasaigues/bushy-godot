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
		private MovementComponent MovementComponent;
		[Node]
		private CharacterCollisionComponent CollisionComponent;

		[Export]
		private CameraFollow cameraFollow;

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
			cameraFollow.SetTarget(this);
			cameraFollow.TargetVelocityGetter = () => MovementComponent.CurrentVelocity;
		}

		public void SetSecondaryTarget(Node2D secondaryTarget, bool positionOverride = false)
		{
			if (secondaryTarget == null)
			{
				cameraFollow.SetOverrideTarget(secondaryTarget);
				cameraFollow.SetMidTarget(secondaryTarget);
				return;
			}
			if (positionOverride)
			{
				cameraFollow.SetOverrideTarget(secondaryTarget);
			}
			else
			{
				cameraFollow.SetMidTarget(secondaryTarget);
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			MovementComponent.UpdateState(this);
			MovementComponent.Move(this);
			if (MovementComponent.IsOnFloor)
			{
				GD.Print(Position.Y);
				cameraFollow.UpdateFloorHeight(Position.Y);
			}
		}
	}

}

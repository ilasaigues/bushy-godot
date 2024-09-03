using Godot;
using GodotUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BushyCore
{	
	[Scene] 
	public partial class HedgeNode : Path2D
	{
        [Export]
        private double Velocity;
        [Node]
        private PathFollow2D PathFollow2D;
        [Node]
        private HedgeStaticBody2D HedgeStaticBody2D;

        private List<MovementComponent> componentsInHedge;
        private Vector2 oldPos;
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
            this.componentsInHedge = new List<MovementComponent>();
            oldPos = PathFollow2D.Position;
        }

        public void SubscribeMovementComponent(MovementComponent mc)
        {
            componentsInHedge.Add(mc);
        }
        public void UnSubscribeMovementComponent(MovementComponent mc)
        {
            Debug.WriteLine("Unsubscribed MC");
            mc.Velocities[MovementComponent.VelocityType.InheritedVelocity] = Vector2.Zero;
            componentsInHedge.Remove(mc);
        }

        public override void _Process(double delta)
        {
            PathFollow2D.Progress += (float) (delta * Velocity);
            var direction = PathFollow2D.Position - oldPos;
            if (direction == Vector2.Zero)
                return;
            var velocity = direction.Normalized() * (float) Velocity *  Transform.Scale;
            
            componentsInHedge.ForEach((mc) => {
                Debug.WriteLine($"Updating inherited velocity to {velocity}");
                mc.Velocities[MovementComponent.VelocityType.InheritedVelocity] = velocity;
            });
            oldPos = PathFollow2D.Position;
        }


    }
}

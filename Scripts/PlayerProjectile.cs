using System.Collections.Generic;
using Godot;

namespace BushyCore
{
    public partial class PlayerProjectile : BaseProjectile
    {
        [Export] Area2D _pushArea;

        private List<Node2D> _nodesInBlastRadius = [];


        protected override void Die(bool impacted = false)
        {
            QueueFree();
            if (impacted)
            {
                foreach (var node in _nodesInBlastRadius)
                {
                    if (node is PlayerController player)
                    {
                        var knockbackVelocity = -_initialVelocity.Normalized();
                        knockbackVelocity.X *= player.CharacterVariables.ProjectileBlastHorizontalKnockback;
                        knockbackVelocity.Y *= Mathf.Abs(player.CharacterVariables.ProjectileBlastVerticalKnockback);
                        player.Knockback(knockbackVelocity);
                    }
                }
            }
        }


        public override void _Ready()
        {
            _pushArea.BodyEntered += OnBodyEntered;
            _pushArea.BodyExited += OnBodyExited;
        }

        public override void _ExitTree()
        {
            _pushArea.BodyEntered -= OnBodyEntered;
            _pushArea.BodyExited -= OnBodyExited;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is PlayerController)
            {
                _nodesInBlastRadius.Add(body);
            }
        }

        private void OnBodyExited(Node2D body)
        {
            _nodesInBlastRadius.Remove(body);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            var collisionInfo = MoveAndCollide(_velocity * (float)delta);
            if (collisionInfo != null)
            {
                OnCollision(collisionInfo);
            }
        }


        protected override void OnCollision(KinematicCollision2D collision)
        {
            Die(true);
        }
    }
}
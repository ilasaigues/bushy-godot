using Godot;

namespace BushyCore
{
    public abstract partial class BaseProjectile : AnimatableBody2D
    {
        public Vector2 Direction;
        protected Vector2 _acceleration;

        protected Vector2 _velocity;
        protected Vector2 _initialVelocity;
        protected float _lifetime;


        public void Fire(float lifetime, Vector2 initialVelocity = default, Vector2 acceleration = default)
        {
            _lifetime = lifetime;
            _initialVelocity = _velocity = initialVelocity;
            _acceleration = acceleration;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (_lifetime <= 0)
            {
                Die();
                return;
            }
            _velocity += _acceleration * (float)delta;
            _lifetime -= (float)delta;
        }

        protected abstract void Die(bool impacted = false);

        protected abstract void OnCollision(KinematicCollision2D collider);
    }
}
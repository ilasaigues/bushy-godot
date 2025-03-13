using System;
using Godot;
using System.Diagnostics;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    public abstract partial class BaseStateOLD : Node2D
    {/*

        [Signal]
        public delegate void EnteredStateEventHandler();
        public double TimeSinceStateStart { get; private set; }
        public Vector2 IntendedDirection { get; private set; }

        protected MovementComponent movementComponent;
        protected CharacterVariables characterVariables;
        protected PlayerInfo actionsComponent;
        protected AnimationComponent animationComponent;
        protected CharacterCollisionComponent collisionComponent;
        protected PlayerStateController stateMachine;

        protected bool IsActive { get; private set; }

        public virtual void InitState(MovementComponent mc,
            CharacterVariables cv,
            PlayerInfo ac,
            AnimationComponent anim,
            CharacterCollisionComponent col,
            PlayerStateController sm)
        {
            this.movementComponent = mc;
            this.characterVariables = cv;
            this.actionsComponent = ac;
            this.animationComponent = anim;
            this.collisionComponent = col;
            this.stateMachine = sm;
        }

        public void StateUpdate(double delta)
        {
            try
            {
                SetIntendedDirection();
                StateUpdateInternal(delta);
            }
            catch (StateInterrupt) { }

            TimeSinceStateStart += delta;
        }
        public virtual void StateUpdateInternal(double delta)
        {
            AnimationUpdate();
        }

        public void StateEnter(params StateConfig.IBaseStateConfig[] configs)
        {
            IsActive = true;
            TimeSinceStateStart = 0;

            StateEnterInternal(configs);

            VelocityUpdate();
        }

        protected virtual void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs) { }

        public virtual void StateExit()
        {
            IsActive = false;
        }

        protected abstract void VelocityUpdate();

        protected abstract void AnimationUpdate();

        public void BaseOnRigidBodyEnter(Node node)
        {
            try
            {
                OnRigidBodyEnter(node);
            }
            catch (StateInterrupt) { }
        }
        public virtual void OnRigidBodyEnter(Node node) { }
        public override string ToString()
        {
            return $"State: {this.GetType().Name}. Time since start: {TimeSinceStateStart}";
        }

        protected void RunAndEndState(Action callable)
        {
            try
            {
                callable.Invoke();
            }
            catch (StateInterrupt) { }
        }

        private void SetIntendedDirection()
        {
            var actionDirection = actionsComponent.MovementDirection.X * Vector2.Right;

            IntendedDirection = actionDirection.Equals(Vector2.Zero)
                ? movementComponent.FacingDirection
                : actionDirection;
        }*/
    }
}

using System;
using Godot;
using System.Diagnostics;
using GodotUtilities;

namespace BushyCore 
{
    [Scene]
    public abstract partial class BaseState : Node2D {

        [Signal]
        public delegate void EnteredStateEventHandler();
        public double TimeSinceStateStart { get; private set; }
        public Vector2 IntendedDirection { get; private set; }

        protected MovementComponent movementComponent;
        protected CharacterVariables characterVariables;
        protected ActionsComponent actionsComponent;
        protected AnimationComponent animationComponent;
        protected CharacterCollisionComponent collisionComponent;

        protected bool IsActive { get; private set; }

        public virtual void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationComponent anim, CharacterCollisionComponent col) {
            this.movementComponent = mc;
            this.characterVariables = cv;
            this.actionsComponent = ac;
            this.animationComponent = anim;
            this.collisionComponent = col;
        }
        
        public void StateUpdate(double delta)
        {
            try
            {   
                SetIntendedDirection();
                StateUpdateInternal(delta);
            }
            catch (StateEndedException) { }

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

        protected virtual void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs) {}

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
            catch (StateEndedException) { }
        }
        public virtual void OnRigidBodyEnter(Node node) {}
        public override string ToString()
        {
            return $"State: {this.GetType().Name}. Time since start: {TimeSinceStateStart}";
        }

        protected void RunAndEndState(Action callable) {
            try
            {
                callable.Invoke();
            }
            catch (StateEndedException) { }
        }

        private void SetIntendedDirection()
        {
            var actionDirection = actionsComponent.MovementDirection.X * Vector2.Right;
            
            IntendedDirection = actionDirection.Equals(Vector2.Zero) 
                ? movementComponent.FacingDirection 
                : actionDirection;
        }
    }
}

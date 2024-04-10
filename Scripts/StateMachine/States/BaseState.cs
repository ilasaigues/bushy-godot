using System;
using Godot;
using System.Diagnostics;

namespace BushyCore 
{
    public abstract partial class BaseState : Node {

        public double TimeSinceStateStart { get; private set; }

        protected MovementComponent movementComponent;
        protected CharacterVariables characterVariables;
        
        protected ActionsComponent actionsComponent;
        public void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac) {
            this.movementComponent = mc;
            this.characterVariables = cv;
            this.actionsComponent = ac;
        }
        
        public void StateUpdate(double delta)
        {
            try
            {
                StateUpdateInternal(delta);
            }
            catch (StateEndedException e) {}

            TimeSinceStateStart += delta;
        }
        public abstract void StateUpdateInternal(double delta);

        public virtual void StateEnter(params StateConfig.IBaseStateConfig[] configs) 
        {
            TimeSinceStateStart = 0;
        }

        public virtual void StateExit() {}

        public override string ToString()
        {
            return $"State: {this.GetType().Name}. Time since start: {TimeSinceStateStart}";
        }
    }
}

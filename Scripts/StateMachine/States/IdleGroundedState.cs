using Godot;
using static MovementComponent;

namespace BushyCore
{
    public partial class IdleGroundedState : BaseChildState<PlayerController, GroundedParentState>
    {
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs) { }

        protected override void ExitStateInternal() { }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            return prevStatus;
        }

        public override bool OnInputAxisChanged(InputAxis axis)
        {
            if (axis == InputManager.Instance.HorizontalAxis && axis.Value != 0)
            {
                throw new StateInterrupt(typeof(WalkState));
            }
            return true;
        }

        public override StateAnimationLevel UpdateAnimation()
        {
            Agent.AnimationComponent.Queue("idle");
            return StateAnimationLevel.Regular;
        }
    }
}
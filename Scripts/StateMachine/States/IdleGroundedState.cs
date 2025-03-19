using Godot;
using static MovementComponent;

namespace BushyCore
{
    public partial class IdleGroundedState : BaseChildState<PlayerController, GroundedParentState>
    {
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            SetupFromConfigs();
        }

        private void SetupFromConfigs(params StateConfig.IBaseStateConfig[] configs)
        {
            bool isLanding = false;
            foreach (var config in configs)
            {
                if (config is StateConfig.InitialGroundedConfig groundedConfig)
                {
                    isLanding = true;
                }
            }

            if (isLanding)
            {
                Agent.AnimationComponent.Stop();
                Agent.AnimationComponent.Play("land");
            }
            if (!isLanding)
            {
                Agent.AnimationComponent.Play("turn");
            }
        }

        protected override void ExitStateInternal() { }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {

            UpdateAnimation();
            return prevStatus;
        }

        public override bool OnInputAxisChanged(InputAxis axis)
        {
            if (axis == InputManager.Instance.HorizontalAxis && axis.Value != 0)
            {
                throw new StateInterrupt<WalkState>();
            }
            return true;
        }

        public override StateAnimationLevel UpdateAnimation()
        {
            if (Agent.AnimationComponent.CurrentAnimation != "idle")
            {
                Agent.AnimationComponent.Queue("idle");
            }
            return StateAnimationLevel.Regular;
        }
    }
}

using Godot;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class WalkState : BaseChildState<PlayerController, GroundedParentState>
    {
        private int _previousDirection = 0;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            SetupFromConfigs(configs);
            _previousDirection = 0;
        }

        private void SetupFromConfigs(params IBaseStateConfig[] configs)
        {
            bool hitTheGroundRunning = false;
            foreach (var config in configs)
            {
                if (config is InitialGroundedConfig groundedConfig)
                {
                    hitTheGroundRunning = true;
                }
            }
            if (!hitTheGroundRunning)
            {
                Agent.AnimationComponent.Queue("run_start");
            }
        }


        protected override void ExitStateInternal()
        {
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            UpdateAnimation();
            return prevStatus;
        }


        public override StateAnimationLevel UpdateAnimation()
        {
            var newDirection = Mathf.Sign(
                Agent.MovementComponent.CurrentVelocity.Rotate(Agent.MovementComponent.FloorAngle)
                .X);

            var anim = Agent.AnimationComponent;
            if (newDirection == _previousDirection) // continue running
            {
                anim.Queue("run");
                return StateAnimationLevel.Regular;
            }
            else // 180° turn
            {
                anim.Stop();
                anim.ClearQueue();
                anim.Play("turn");
                anim.Queue("run_start");
                anim.Queue("run");
                _previousDirection = newDirection;
            }
            return StateAnimationLevel.Regular;
        }

        public override bool OnInputAxisChanged(InputAxis axis)
        {
            if (axis == InputManager.Instance.HorizontalAxis && axis.Value == 0)
            {
                Agent.AnimationComponent.ClearQueue();
                Agent.AnimationComponent.Queue("idle");
                throw new StateInterrupt(typeof(IdleGroundedState));
            }
            return true;
        }

    }
}
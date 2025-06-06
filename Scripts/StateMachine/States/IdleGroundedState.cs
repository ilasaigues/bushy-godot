using System;
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
                Agent.AnimController.SetTrigger(PlayerController.AnimConditions.LandTrigger);
            }
        }

        protected override void ExitStateInternal() { }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            CheckVelocity();
            return prevStatus;
        }

        protected void CheckVelocity()
        {
            if (Math.Abs(Agent.MovementComponent.Velocities[VelocityType.MainMovement].X) != 0 && Agent.MovementInputVector.X != 0)
            {
                throw new StateInterrupt(typeof(IdleGroundedState));
            }
        }

        protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            if (axis == InputManager.Instance.HorizontalAxis && axis.Value != 0)
            {
                ChangeState<WalkState>();
            }
            return true;
        }
    }
}
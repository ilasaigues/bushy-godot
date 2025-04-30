
using System;
using Godot;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class WalkState : BaseChildState<PlayerController, GroundedParentState>
    {
        private int _previousDirection = 0;

        protected override void EnterStateInternal(params IBaseStateConfig[] configs)
        {
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Running, true);
            _previousDirection = Mathf.Sign(
                Agent.MovementComponent.CurrentVelocity.Rotate(Agent.MovementComponent.FloorAngle).X);
        }

        protected override void ExitStateInternal()
        {
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Running, false);
            _previousDirection = 0;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            UpdateAnimation();
            CheckVelocity();
            return prevStatus;
        }

        protected void CheckVelocity()
        {
            if (Math.Abs(Agent.MovementComponent.Velocities[VelocityType.MainMovement].X) < 2 && Agent.MovementInputVector.X == 0)
            {
                Agent.PlayerInfo.IsInDashMode = false;
                throw new StateInterrupt(typeof(IdleGroundedState));
            }
        }

        public override StateAnimationLevel UpdateAnimation()
        {
            var newDirection = Mathf.Sign(
                Agent.MovementComponent.Velocities[VelocityType.MainMovement].X);
            if (newDirection != 0)
            {
                var anim = Agent.AnimController;
                if (newDirection * _previousDirection == -1)
                {
                    Agent.PlayerInfo.IsInDashMode = false;
                    //GD.PrintRich("[color=red]BIJA[/color]");
                    anim.SetTrigger(PlayerController.AnimConditions.TurnTrigger);
                    _previousDirection = newDirection;
                }
            }
            return StateAnimationLevel.Regular;
        }

        /*protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            if (axis == InputManager.Instance.HorizontalAxis && axis.Value == 0)
            {
                throw new StateInterrupt(typeof(IdleGroundedState));
            }
            return true;
        }
*/
    }
}
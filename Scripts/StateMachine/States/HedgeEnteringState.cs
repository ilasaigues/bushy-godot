
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeEnteringState : BaseChildState<PlayerController, HedgeParentState>
    {
        [Export]
        private Timer EnteringTimer;
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            ParentState.xAxisMovement.SetInitVel(Agent.MovementComponent.CurrentVelocity.X);
            ParentState.yAxisMovement.SetInitVel(Agent.MovementComponent.CurrentVelocity.Y);

            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            EnteringTimer.WaitTime = Agent.CharacterVariables.HedgeEnteringWaitTime;
            RemoveControls();
        }

        void EnteringTimerTimeout()
        {
            EnteringTimer.Timeout -= EnteringTimerTimeout;
            ReturnControls();
        }

        private void RemoveControls()
        {
            EnteringTimer.Timeout += EnteringTimerTimeout;
            EnteringTimer.Start();

            if (Mathf.Abs(ParentState.xAxisMovement.Velocity) > Agent.CharacterVariables.HedgeMovementSpeed)
                ParentState.xAxisMovement = ParentState.xAxisMovement.ToBuilder().Copy()
                    .Direction(() => 0)
                    .Build();

            // The positive value is intentional. Only remove controls if the character is falling downwards
            if (ParentState.yAxisMovement.Velocity > Agent.CharacterVariables.HedgeMovementSpeed)
                ParentState.yAxisMovement = ParentState.yAxisMovement.ToBuilder().Copy()
                    .Direction(() => 0)
                    .Build();
        }

        private void ReturnControls()
        {
            ParentState.xAxisMovement = ParentState.xAxisMovement.ToBuilder().Copy()
                .Direction(() => { return Agent.MovementInputVector.X; })
                .Build();
            ParentState.yAxisMovement = ParentState.yAxisMovement.ToBuilder().Copy()
                .Direction(() => { return Agent.MovementInputVector.Y; })
                .Build();
        }

        protected override void ExitStateInternal() { }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (EnteringTimer.TimeLeft <= 0)
            {
                throw new StateInterrupt<HedgeMoveState>();
            }
            return prevStatus;
        }
    }
}
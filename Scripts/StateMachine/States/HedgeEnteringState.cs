
using Godot;
using GodotUtilities;

namespace BushyCore
{
    public partial class HedgeEnteringState : BasePlayerState, IChildState<PlayerController, HedgeState>
    {
        public HedgeState ParentState { get; set; }

        [Node]
        private Timer EnteringTimer;
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            RemoveControls();
        }

        void EnteringTimerTimeout()
        {
            EnteringTimer.Timeout -= EnteringTimerTimeout;
            ReturnControls();
            if (Agent.MovementInputVector.Length() == 0)
            {
                throw new StateInterrupt<HedgeIdleState>();
            }
            else
            {
                throw new StateInterrupt<HedgeMoveState>();
            }
        }

        private void RemoveControls()
        {
            EnteringTimer.Start();
            EnteringTimer.Timeout += EnteringTimerTimeout;

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

        protected override void ExitStateInternal()
        {
            EnteringTimer.Timeout -= EnteringTimerTimeout;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            return prevStatus;
        }
    }
}
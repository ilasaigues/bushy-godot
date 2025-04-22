
using System;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeEnteringState : BaseChildState<PlayerController, HedgeParentState>
    {
        private Vector2 _targetVelocity;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            _targetVelocity = (Agent.MovementComponent.InsideHedgeDirection.Normalized() * Agent.CharacterVariables.MaxHedgeEnterSpeed).PrintInPlace("Target velocity: {0}");
            SetupFromConfigs(configs);
            RemoveControls();
        }

        void SetupFromConfigs(params StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.InitialHedgeConfig hedgeConfig)
                {
                    ParentState.SetVelocity(hedgeConfig.Direction.PrintInPlace("Starting velocity: {0}"));
                }
            }
        }

        private void RemoveControls()
        {


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
            ParentState.SetVelocity(ParentState.CurrentVelocity.PrintInPlace("Lerping: {0}").Slerp(
                                _targetVelocity,
                                0.33333f));
            if (Agent.MovementComponent.InsideHedgeDirection == Vector2.Zero
                    || TimeSinceStateEntered >= TimeSpan.FromSeconds(Agent.CharacterVariables.HedgeEnteringWaitTime))
            {
                ReturnControls();
                throw StateInterrupt.New<HedgeMoveState>();
            }
            return prevStatus;
        }
    }
}
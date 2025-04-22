
using System;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
    public partial class HedgeExitState : BaseChildState<PlayerController, HedgeParentState>
    {
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            ParentState.SetVelocity(Agent.MovementComponent.OutsideHedgeDirection
                                * Agent.CharacterVariables.HedgeMovementSpeed);
            RemoveControls();
        }

        private void RemoveControls()
        {
            ParentState.xAxisMovement = ParentState.xAxisMovement.ToBuilder().Copy()
                .Direction(() => 0)
                .Build();

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
            ReturnControls();
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (Agent.MovementComponent.OutsideHedgeDirection == Vector2.Zero || TimeSinceStateEntered > TimeSpan.FromMilliseconds(150))
            {
                ParentState.OnHedgeExit();
            }
            return prevStatus;
        }
    }
}
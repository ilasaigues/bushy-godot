
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
            Agent.CollisionComponent.ToggleHedgeCollision(false);
            UpdateTargetVelocity();
            SetupFromConfigs(configs);
            RemoveControls();
        }

        void SetupFromConfigs(params StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    _targetVelocity = velocityConfig.Velocity;
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

        private void UpdateTargetVelocity()
        {
            _targetVelocity = Agent.MovementComponent.InsideHedgeDirection.Normalized() * Agent.CharacterVariables.MaxHedgeEnterSpeed;

        }

        protected override void ExitStateInternal() { }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            UpdateTargetVelocity();
            ParentState.SetVelocity(ParentState.CurrentVelocity.Slerp(
                                _targetVelocity,
                                0.33333f));
            if (TimeSinceStateEntered > TimeSpan.FromSeconds(0.1) && Agent.MovementComponent.HedgeState == HedgeOverlapState.Outside)
            {
                ReturnControls();
                throw StateInterrupt.New<FallState>();
            }

            if (Agent.MovementComponent.HedgeState == HedgeOverlapState.Complete)
            {
                ReturnControls();
                Agent.Position += Agent.MovementComponent.InsideHedgeDirection;
                throw StateInterrupt.New<HedgeMoveState>();
            }
            return prevStatus;
        }
    }
}
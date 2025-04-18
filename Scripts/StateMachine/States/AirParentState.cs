using System;
using System.Linq;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class AirParentState : BaseParentState<PlayerController, AirParentState>
    {

        [Export] public Timer CoyoteJumpTimer { get; private set; }

        public AxisMovement XAxisMovement { get; private set; }

        public float VerticalVelocity { get; set; }

        public float TargetHorizontalVelocity { get; set; }

        public bool CanCoyoteJump { get; set; }

        public bool CanFallIntoHedge { get; set; }

        public override void SetAgent(PlayerController playerController)
        {
            base.SetAgent(playerController);
            this.XAxisMovement = new AxisMovement.Builder()
                .Acc(Agent.CharacterVariables.AirHorizontalAcceleration)
                .Dec(Agent.CharacterVariables.AirHorizontalDeceleration)
                .Speed(Agent.CharacterVariables.AirHorizontalMovementSpeed)
                .OverDec(Agent.CharacterVariables.AirHorizontalOvercappedDeceleration)
                .TurnDec(Agent.CharacterVariables.HorizontalTurnDeceleration)
                .Movement(Agent.MovementComponent)
                .Direction(() => { return InputManager.Instance.HorizontalAxis.Value; })
                .ColCheck((dir) => { return Agent.MovementComponent.IsOnWall; })
                .Variables(Agent.CharacterVariables)
                .Build();
        }


        protected override void EnterStateInternal(params IBaseStateConfig[] configs)
        {
            XAxisMovement.SetInitVel(Agent.MovementComponent.Velocities[VelocityType.MainMovement].X);

            VerticalVelocity = 0;
            CanFallIntoHedge = false;
            CanCoyoteJump = Agent.PlayerInfo.CanJump;

            if (CanCoyoteJump)
            {
                CoyoteJumpTimer.WaitTime = Agent.CharacterVariables.JumpCoyoteTime;
                CoyoteJumpTimer.Start();
            }

            XAxisMovement.OvershootDec(true);

            Agent.CollisionComponent.CallDeferred(
                 CharacterCollisionComponent.MethodName.SwitchShape,
                 (int)CharacterCollisionComponent.ShapeMode.CILINDER);

            SetupFromConfigs(configs);

            Agent.AnimController.SetCondition(PlayerController.AnimConditions.OnAir, true);

            if (CanFallIntoHedge)
            {
                Agent.CollisionComponent.ToggleHedgeCollision(false);
            }
        }

        private void SetupFromConfigs(IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is InitialVelocityVectorConfig velocityConfig)
                {
                    VerticalVelocity = velocityConfig.Velocity.Y;
                    TargetHorizontalVelocity = velocityConfig.Velocity.X;
                    XAxisMovement.OvershootDec(velocityConfig.DoesDecelerate);
                    CanFallIntoHedge = velocityConfig.CanEnterHedge;
                    Agent.PlayerInfo.IsInDashMode = !velocityConfig.DoesDecelerate && velocityConfig.CanEnterHedge;
                }
            }
        }

        protected override void ExitStateInternal()
        {
            base.ExitStateInternal();
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.OnAir, false);
            if (!Agent.MovementComponent.IsInHedge)
                Agent.CollisionComponent.ToggleHedgeCollision(true);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus processConfig, double delta)
        {
            if (processConfig.StateExecutionResult != StateExecutionResult.Block)
            {
                if (CanFallIntoHedge && Agent.MovementComponent.IsInHedge)
                {
                    throw StateInterrupt.New<HedgeEnteringState>(false, new StateConfig.InitialHedgeConfig(Agent.MovementComponent.HedgeEntered, (float)XAxisMovement.Velocity * Vector2.Right));
                }

                if (VerticalVelocity < 0f && Agent.MovementComponent.IsOnCeiling)
                    VerticalVelocity = 0;

                if (Agent.MovementComponent.IsOnFloor && VerticalVelocity >= 0)
                {
                    bool IsJumpBuffered = InputManager.Instance.JumpAction.TimeSinceLastPressed <= Agent.CharacterVariables.JumpBufferTime;
                    if (Agent.MovementInputVector.X != 0)
                    {
                        throw StateInterrupt.New<WalkState>(false,
                            new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                    else
                    {
                        throw StateInterrupt.New<IdleGroundedState>(false,
                            new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                }
                if (Agent.MovementComponent.CurrentVelocity.Y > 0)
                {
                    Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
                }
            }

            if (processConfig.CanChangeAnimation)
            {
                UpdateAnimation();
            }

            if (processConfig.CanMoveHorizontal)
            {
                XAxisMovement.HandleMovement(delta);
            }
            if (processConfig.CanMoveVertical)
            {
                VelocityUpdate();
            }
            if (processConfig.StateExecutionResult == StateExecutionResult.Block)
            {
                return processConfig;
            }
            else
            {
                return ProcessSubState(processConfig, delta);
            }
        }

        public override StateAnimationLevel UpdateAnimation()
        {
            if (Agent.MovementComponent.CurrentVelocity.Y > 0
            && !Agent.AnimController.GetCondition(PlayerController.AnimConditions.Falling))
            {
                Agent.AnimController.SetCondition(PlayerController.AnimConditions.Falling, true);
            }
            return StateAnimationLevel.Regular;
        }

        public void HandleGravity(double deltaTime)
        {
            VerticalVelocity = Mathf.Min(Agent.CharacterVariables.AirTerminalVelocity, VerticalVelocity + GetGravity() * (float)deltaTime);
        }

        protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            return CurrentSubState.OnInputAxisChanged(axis);
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction Action)
        {
            if (Agent.PlayerInfo.CanDash
                && actionType == InputAction.InputActionType.InputPressed
                && Action == InputManager.Instance.DashAction)
            {
                throw StateInterrupt.New<DashState>(false, InitialVelocityVector(Agent.MovementInputVector, false, true));
            }

            if (actionType == InputAction.InputActionType.InputPressed
                && Action == InputManager.Instance.AttackAction)
            {
                throw StateInterrupt.New<AttackAirState>(false);
            }

            return CurrentSubState.OnInputButtonChanged(actionType, Action);
        }

        float GetGravity()
        {
            if (VerticalVelocity <= Agent.CharacterVariables.AirSpeedThresholds.Y)
            {
                return Agent.CharacterVariables.AirGravity;
            }
            else if (VerticalVelocity <= Agent.CharacterVariables.AirSpeedThresholds.X)
            {
                return Agent.CharacterVariables.AirApexGravity;
            }
            else
            {
                return Agent.CharacterVariables.AirGravity;
            }
        }

        protected void VelocityUpdate()
        {
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float)VerticalVelocity);
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = (float)XAxisMovement.Velocity * Vector2.Right;
        }

        public void OnJumpCoyoteTimerTimeout()
        {
            CanCoyoteJump = false;
        }
    }
}
using System;
using System.Linq;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;
using Microsoft.CodeAnalysis;

namespace BushyCore
{
    public partial class AirParentState : BaseParentState<PlayerController, AirParentState>
    {

        [Export] public Timer CoyoteJumpTimer { get; private set; }

        public AxisMovement XAxisMovement { get; private set; }

        public float VerticalVelocity { get; set; }

        public float TargetHorizontalVelocity { get; set; }

        public bool CanCoyoteJump { get; set; }

        public bool IgnorePlatforms = false;

        private float _lastPlatformHeight = 0;

        public override void SetAgent(PlayerController playerController)
        {
            base.SetAgent(playerController);
            XAxisMovement = new AxisMovement.Builder()
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
            Agent.PlayerInfo.CanFallIntoHedge = false;
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

            Agent.MovementComponent.IsOnFloor = false;
            Agent.MovementComponent.FloorHeightCheckEnabled = false;

            SetupFromConfigs(configs);

            Agent.AnimController.SetCondition(PlayerController.AnimConditions.OnAir, true);
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
                    Agent.PlayerInfo.CanFallIntoHedge = velocityConfig.CanEnterHedge;
                    Agent.PlayerInfo.IsInDashMode = !velocityConfig.DoesDecelerate && velocityConfig.CanEnterHedge;
                    if (Agent.PlayerInfo.IsInDashMode && TargetHorizontalVelocity == 0)
                    {
                        TargetHorizontalVelocity = Agent.PlayerInfo.LookDirection * Agent.CharacterVariables.DashJumpSpeed;
                    }
                    XAxisMovement.SetInitVel(TargetHorizontalVelocity);
                }
                if (config is PlatformDropConfig)
                {
                    SetPlatformCollision(false);
                }
            }
        }


        private void SetPlatformCollision(bool enabled)
        {
            if (enabled)
            {
                IgnorePlatforms = false;
                _lastPlatformHeight = 0;
                Agent.CollisionComponent.TogglePlatformCollision(true);
            }
            else
            {
                IgnorePlatforms = true;
                _lastPlatformHeight = Agent.Position.Y;
                Agent.CollisionComponent.TogglePlatformCollision(false);
            }
        }

        public override bool Message(Enum message, params object[] args)
        {
            if (message is PlayerController.StateMessage stateMessage)
            {
                var velocity = (Vector2)args[0];
                switch (stateMessage)
                {
                    case PlayerController.StateMessage.Knockback:
                        if (Mathf.Abs(velocity.Y) > Mathf.Abs(velocity.X) && velocity.Y < 0) // if the velocity is vertical 
                        {
                            velocity.Y = Agent.CharacterVariables.JumpSpeed;
                            velocity.X = Agent.MovementComponent.Velocities[VelocityType.MainMovement].X;
                        }
                        TargetHorizontalVelocity = velocity.X;
                        XAxisMovement.SetInitVel(TargetHorizontalVelocity);
                        ChangeState<FallState>(false, new InitialVelocityVectorConfig(velocity, !Agent.PlayerInfo.IsInDashMode, Agent.PlayerInfo.CanFallIntoHedge));
                        break;
                }
            }
            return true;
        }

        protected override void ExitStateInternal()
        {
            base.ExitStateInternal();
            SetPlatformCollision(true);
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.OnAir, false);
            if (!Agent.PlayerInfo.IsInHedge)
                Agent.CollisionComponent.ToggleHedgeCollision(true);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus processConfig, double delta)
        {
            if (processConfig.StateExecutionResult != StateExecutionResult.Block)
            {
                if (IgnorePlatforms)
                {
                    if (Agent.Position.Y < _lastPlatformHeight || Agent.Position.Y > _lastPlatformHeight + 10)
                    {
                        SetPlatformCollision(true);
                    }
                }

                if (Agent.PlayerInfo.CanFallIntoHedge && Agent.MovementComponent.HedgeState != HedgeOverlapState.Outside)
                {
                    ChangeState<HedgeEnteringState>(false, new InitialVelocityVectorConfig(Agent.MovementComponent.CurrentVelocity));
                }

                if (!IgnorePlatforms && Agent.MovementComponent.IsOnFloor && VerticalVelocity >= 0)
                {
                    bool IsJumpBuffered = InputManager.Instance.JumpAction.TimeSinceLastPressed <= Agent.CharacterVariables.JumpBufferTime;
                    if (Agent.MovementInputVector.X != 0)
                    {
                        ChangeState<WalkState>(false,
                            new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                    else
                    {
                        ChangeState<IdleGroundedState>(false,
                            new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                }

                if (VerticalVelocity < 0f && Agent.MovementComponent.IsOnCeiling)
                {
                    VerticalVelocity = Mathf.Min(VerticalVelocity, 0);
                    ChangeState<FallState>();
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
            if (Agent.PlayerInfo.IsAttacking)
            {
                return true;
            }
            return CurrentSubState.OnInputAxisChanged(axis);
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction action)
        {
            if (Agent.PlayerInfo.IsAttacking)
            {
                return true;
            }
            if (Agent.PlayerInfo.CanDash
                && actionType == InputAction.InputActionType.InputPressed
                && action == InputManager.Instance.DashAction)
            {
                ChangeState<DashState>(false, InitialVelocityVector(Agent.MovementInputVector, false, true));
            }

            if (actionType == InputAction.InputActionType.InputPressed
                && action == InputManager.Instance.AttackAction)
            {
                ChangeState<AttackAirState>(false);
            }

            if (actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.BurstAction)
            {
                var direction = Agent.MovementInputVector;
                if (direction == Vector2.Zero) // no input pressed, assume horizontal
                {
                    direction = Agent.PlayerInfo.LookDirection * Vector2.Right;
                }
                else // input is pressed, sanizite input in case of multiple axes being active at the same time
                {
                    if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
                    {
                        direction.Y = 0;
                        direction = direction.Normalized();
                    }
                }
                ChangeState<ProjectileAttackState>(false, new FireProjectileConfig(direction, Agent.PlayerInfo.LookDirection == -1));
            }

            return CurrentSubState.OnInputButtonChanged(actionType, action);
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
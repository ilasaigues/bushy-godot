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
                }
            }
        }

        protected override void ExitStateInternal()
        {
            if (!Agent.MovementComponent.IsInHedge)
                Agent.CollisionComponent.ToggleHedgeCollision(true);
            ExitSubState();
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus processConfig, double delta)
        {
            if (processConfig.StateExecutionResult != StateExecutionResult.Block)
            {
                if (CanFallIntoHedge && Agent.MovementComponent.IsInHedge)
                {
                    throw new StateInterrupt<HedgeEnteringState>(new StateConfig.InitialHedgeConfig(Agent.MovementComponent.HedgeEntered, (float)XAxisMovement.Velocity * Vector2.Right));
                }

                if (VerticalVelocity < 0f && Agent.MovementComponent.IsOnCeiling)
                    VerticalVelocity = 0;

                if (Agent.MovementComponent.IsOnFloor && VerticalVelocity > 0)
                {
                    if (processConfig.CanChangeAnimation)
                    {
                        Agent.AnimationComponent.Play("land");
                    }
                    bool IsJumpBuffered = InputManager.Instance.JumpAction.TimeSinceLastPressed <= Agent.CharacterVariables.JumpBufferTime;
                    if (TargetHorizontalVelocity != 0)
                    {
                        throw new StateInterrupt(typeof(WalkState), new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                    else
                    {
                        throw new StateInterrupt(typeof(IdleGroundedState), new InitialGroundedConfig(IsJumpBuffered, XAxisMovement.HasOvershootDeceleration));
                    }
                }
                if (Agent.MovementComponent.CurrentVelocity.Y > 0)
                {
                    Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
                }
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
            if (Agent.MovementComponent.CurrentVelocity.Y > 0 && Agent.AnimationComponent.CurrentAnimation != "fall")
            {
                Agent.AnimationComponent.ClearQueue();
                Agent.AnimationComponent.Play("peak");
                Agent.AnimationComponent.Queue("fall");
            }
            return StateAnimationLevel.Regular;
        }

        public void HandleGravity(double deltaTime)
        {
            VerticalVelocity = Mathf.Min(Agent.CharacterVariables.AirTerminalVelocity, VerticalVelocity + GetGravity() * (float)deltaTime);
        }

        public override bool OnInputAxisChanged(InputAxis axis)
        {
            return CurrentSubState.OnInputAxisChanged(axis);
        }

        public override bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            if (actionType == InputAction.InputActionType.InputPressed && Action == InputManager.Instance.DashAction)
            {
                throw new StateInterrupt<DashState>(InitialVelocityVector(Agent.MovementInputVector, false, true));
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
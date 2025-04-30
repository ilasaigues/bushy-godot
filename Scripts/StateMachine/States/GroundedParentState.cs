

using System;
using System.Linq;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class GroundedParentState : BaseParentState<PlayerController, GroundedParentState>
    {
        [Node]
        public Timer DashCooldownTimer;
        public double VerticalVelocity;

        public AxisMovement HorizontalAxisMovement;


        private bool _jumpBuffered = false;

        public override void SetAgent(PlayerController playerController)
        {
            base.SetAgent(playerController);
            HorizontalAxisMovement = new AxisMovement.Builder()
                .Acc(Agent.CharacterVariables.GroundHorizontalAcceleration)
                .Dec(Agent.CharacterVariables.GroundHorizontalDeceleration)
                .Speed(Agent.CharacterVariables.GroundHorizontalMovementSpeed)
                .OverDec(Agent.CharacterVariables.GroundHorizontalOvercappedDeceleration)
                .TurnDec(Agent.CharacterVariables.HorizontalTurnDeceleration)
                .Movement(Agent.MovementComponent)
                .Direction(() => { return InputManager.Instance.HorizontalAxis.Value; })
                .ColCheck((dir) => { return Agent.MovementComponent.IsOnWall; })
                .Variables(Agent.CharacterVariables)
                .Build();
        }


        protected override void EnterStateInternal(params IBaseStateConfig[] configs)
        {
            HorizontalAxisMovement.SetInitVel(Agent.MovementComponent.Velocities[VelocityType.MainMovement].X);
            Agent.CollisionComponent.ToggleHedgeCollision(true);

            _jumpBuffered = false;
            Agent.PlayerInfo.CanJump = true;
            Agent.PlayerInfo.DashEnabled = true;
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            HorizontalAxisMovement.OvershootDec(true);

            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Grounded, true);


            Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);

            SetupFromConfigs(configs);
        }
        private void SetupFromConfigs(IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is InitialGroundedConfig groundedConfig)
                {
                    if (groundedConfig.IsJumpBuffered)
                    {
                        _jumpBuffered = true;
                    }
                    HorizontalAxisMovement.OvershootDec(groundedConfig.DoesDecelerate);
                }
            }
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction action)
        {
            if (Agent.PlayerInfo.CanJump && actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.JumpAction)
            {
                if (Agent.MovementComponent.CanDropFromPlatform && Agent.MovementInputVector.Y > 0 && Agent.MovementInputVector.Dot(Vector2.Down * 10) > 0.707106781187) //Sin(45°)
                {
                    Agent.MovementComponent.IsOnFloor = false;
                    throw StateInterrupt.New<FallState>(false, new PlatformDropConfig());
                }
                else
                {
                    DoJump();
                }
            }

            if (Agent.PlayerInfo.CanDash
                && actionType == InputAction.InputActionType.InputPressed
                && action == InputManager.Instance.DashAction)
            {
                throw StateInterrupt.New<DashState>(false, InitialVelocityVector(Agent.MovementInputVector, false, true));
            }

            if (actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.AttackAction)
            {
                throw StateInterrupt.New<AttackGroundState>(false);
            }
            return CurrentSubState.OnInputButtonChanged(actionType, action);
        }

        private void DoJump()
        {
            throw StateInterrupt.New<JumpState>(false,
                    StateConfig.InitialVelocityVector(Agent.MovementComponent.Velocities[VelocityType.MainMovement]));
        }

        protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            if (Agent.MovementComponent.CanDropFromPlatform && InputManager.Instance.JumpAction.Pressed)
            {
                if (Agent.MovementInputVector.Y > 0 && Agent.MovementInputVector.Dot(Vector2.Down * 10) > 0.707106781187) //Sin(45°)
                {
                    Agent.MovementComponent.IsOnFloor = false;
                    throw StateInterrupt.New<FallState>(false, new PlatformDropConfig());
                }
            }
            return CurrentSubState.OnInputAxisChanged(axis);
        }

        protected void UpdateVelocity(StateExecutionStatus prevStatus)
        {
            if (!prevStatus.CanMoveHorizontal)
            {
                Agent.MovementComponent.Velocities[VelocityType.MainMovement] = Vector2.Zero;
                Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
                return;
            }

            var downwardVel = Agent.MovementComponent.IsOnEdge ? 0 : 15;
            var slopeVerticalComponent = Mathf.Tan(Agent.MovementComponent.FloorAngle) * (float)HorizontalAxisMovement.Velocity;
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Agent.MovementComponent.FloorAngle != 0 ?
                Agent.MovementComponent.FloorNormal * (float)VerticalVelocity * downwardVel
                : Vector2.Zero;
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float)HorizontalAxisMovement.Velocity, slopeVerticalComponent);
        }

        protected override void ExitStateInternal()
        {
            base.ExitStateInternal();
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Grounded, false);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (_jumpBuffered)
            {
                DoJump();
            }
            // Having a downwards velocity constantly helps snapping the character to the ground
            // We have to keep in mind that while using move and slide this WILL impact the characterś movement horizontally
            VerticalVelocity = -10f;
            HandleMovement(delta);
            UpdateVelocity(prevStatus);
            CheckTransitions();
            return ProcessSubState(prevStatus, delta);
        }


        void HandleMovement(double deltaTime)
        {
            HorizontalAxisMovement.HandleMovement(deltaTime);
        }


        void CheckTransitions()
        {
            if (Agent.MovementComponent.SnappedToFloor)
            {
                return;
            }

            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            throw StateInterrupt.New<FallState>();
        }
    }
}


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

            Agent.MovementComponent.FloorHeightCheckEnabled = true;
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
            if (Agent.PlayerInfo.IsAttacking)
            {
                return true;
            }
            if (Agent.PlayerInfo.CanJump && actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.JumpAction)
            {
                if (Agent.MovementComponent.CanDropFromPlatform && Agent.MovementInputVector.Y > 0 && Agent.MovementInputVector.Dot(Vector2.Down * 10) > 0.707106781187) //Sin(45°)
                {
                    Agent.MovementComponent.IsOnFloor = false;
                    ChangeState<FallState>(false, new PlatformDropConfig());
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
                if (Agent.MovementComponent.IsStandingOnHedge && Agent.MovementInputVector.Dot(Vector2.Down) > 0.4)
                {
                    ChangeState<HedgeEnteringState>(false, InitialVelocityVector(Agent.MovementInputVector.Normalized() * Agent.CharacterVariables.MaxHedgeEnterSpeed, false, true));
                }
                else
                {
                    ChangeState<DashState>(false, InitialVelocityVector(Agent.MovementInputVector, false, true));
                }
            }

            if (actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.AttackAction)
            {
                ChangeState<AttackGroundState>(false);
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
                    if (Mathf.Abs(direction.X) >= Mathf.Abs(direction.Y))
                    {
                        direction.Y = 0;
                        direction = direction.Normalized();
                    }
                }
                if (direction.Y <= 0)
                {
                    ChangeState<ProjectileAttackState>(false, new FireProjectileConfig(direction, Agent.PlayerInfo.LookDirection == -1));
                }
            }
            return CurrentSubState.OnInputButtonChanged(actionType, action);
        }

        public override bool Message(Enum message, params object[] args)
        {
            if (message is PlayerController.StateMessage stateMessage)
            {
                var velocity = (Vector2)args[0];
                switch (stateMessage)
                {
                    case PlayerController.StateMessage.Knockback:
                        if (Mathf.Abs(velocity.Y) > Mathf.Abs(velocity.X) && velocity.Y < 0)
                        {
                            velocity.Y = Agent.CharacterVariables.JumpSpeed;
                        }
                        ChangeState<JumpState>(false, new InitialVelocityVectorConfig(velocity));
                        break;
                }
            }
            return true;
        }


        private void DoJump()
        {
            if (Agent.PlayerInfo.IsInDashMode && InputManager.Instance.DashAction.Pressed)
            {
                var jumpVelocity = Agent.MovementComponent.Velocities[VelocityType.MainMovement];
                jumpVelocity.X = Mathf.Sign(jumpVelocity.X) * Agent.CharacterVariables.DashJumpSpeed;
                ChangeState<JumpState>(true, new InitialVelocityVectorConfig(jumpVelocity, false, true));
            }
            ChangeState<JumpState>(false,
                    StateConfig.InitialVelocityVector(Agent.MovementComponent.Velocities[VelocityType.MainMovement]));
        }

        protected override bool OnInputAxisChangedInternal(InputAxis axis)
        {
            if (Agent.PlayerInfo.IsAttacking)
            {
                return true;
            }
            if (Agent.MovementComponent.CanDropFromPlatform && InputManager.Instance.JumpAction.Pressed)
            {
                if (Agent.MovementInputVector.Y > 0 && Agent.MovementInputVector.Dot(Vector2.Down * 10) > 0.707106781187) //Sin(45°)
                {
                    Agent.MovementComponent.IsOnFloor = false;
                    ChangeState<FallState>(false, new PlatformDropConfig());
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
                : Vector2.Down * downwardVel;
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
            ChangeState<FallState>();
        }
    }
}
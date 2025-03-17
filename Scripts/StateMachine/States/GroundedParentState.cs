

using System;
using System.Collections.Generic;
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
            this.HorizontalAxisMovement = new AxisMovement.Builder()
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


        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            SetCanDash();

            HorizontalAxisMovement.SetInitVel(Agent.MovementComponent.Velocities[VelocityType.MainMovement].X);

            _jumpBuffered = false;
            Agent.PlayerInfo.CanJump = true;
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            HorizontalAxisMovement.OvershootDec(true);

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

        public override bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction action)
        {
            if (Agent.PlayerInfo.CanJump && actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.JumpAction)
            {
                DoJump();
            }

            if (actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.DashAction)
            {
                throw new StateInterrupt<DashState>(InitialVelocityVector(Agent.MovementInputVector, false, true));
            }
            return CurrentSubState.OnInputButtonChanged(actionType, action);
        }

        private void DoJump()
        {
            throw new StateInterrupt<JumpState>(
                    StateConfig.InitialVelocityVector(new Vector2((float)HorizontalAxisMovement.Velocity, Agent.CharacterVariables.JumpSpeed)));
        }

        public override bool OnInputAxisChanged(InputAxis axis)
        {
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

        void SetCanDash()
        {
            var dashCdRemaining = Agent.CharacterVariables.DashCooldown - (Time.GetTicksMsec() - Agent.PlayerInfo.LastDashTime);
            if (dashCdRemaining <= 0)
                Agent.PlayerInfo.CanDash = true;
            else
                DashCooldownTimer.WaitTime = dashCdRemaining / 1000;

            if (!Agent.PlayerInfo.CanDash)
                DashCooldownTimer.Start();
        }

        protected override void ExitStateInternal()
        {
            Agent.PlayerInfo.CanDash = true;
            ExitSubState();
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
            UpdateAnimation();
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
            throw new StateInterrupt<FallState>();
        }

        public override StateAnimationLevel UpdateAnimation()
        {

            float direction = Agent.MovementComponent.CurrentVelocity.X;

            var anim = Agent.AnimationComponent;

            if (direction != 0)
            {
                if (direction * HorizontalAxisMovement.Velocity < 0)
                {
                    anim.Play("turn");
                    anim.ClearQueue();
                    anim.Queue("run_start");
                    anim.Queue("run");
                }
                else if (anim.CurrentAnimation != "run")
                {
                    if (anim.CurrentAnimation == "idle") anim.Play("run_start");
                    else anim.Queue("run_start");
                    anim.ClearQueue();
                    anim.Queue("run");
                }

            }
            else if (anim.CurrentAnimation == "run")
            {
                anim.Play("turn");
                anim.ClearQueue();
                anim.Queue("idle");
            }
            else
            {
                anim.Queue("idle");
            }
            return StateAnimationLevel.Regular;
        }

        /*

                public override void _Ready()
                {
                    base._Ready();

                    this.AddToGroup();
                    this.WireNodes();
                }



                void DashCooldownTimerTimeout()
                {
                    if (!this.IsActive) return;

                    actionsComponent.CanDash = true;
                }

               
        */
    }
}
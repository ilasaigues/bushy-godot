

using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class GroundedParentState : BasePlayerState, IParentState<PlayerController, GroundedParentState>
    {
        [Node]
        public Timer DashCooldownTimer;
        public double VerticalVelocity;

        public AxisMovement HorizontalAxisMovement;

        public IChildState<PlayerController, GroundedParentState> CurrentSubState { get; set; }
        [Export] public BaseState<PlayerController>[] SubStates { get; set; }


        private IChildState<PlayerController, GroundedParentState> _nextState = null;
        private IBaseStateConfig[] _nextConfigs = null;
        public override bool TryChangeToState(Type type, params IBaseStateConfig[] configs)
        {
            foreach (var subState in SubStates)
            {
                if (subState.GetType() == type && subState is IChildState<PlayerController, GroundedParentState> childState)
                {
                    _nextState = childState;
                    return true;
                }
            }
            return false;
        }

        public override void _Ready()
        {
            foreach (var genericChild in GetChildren())
            {
                if (genericChild is IChildState<PlayerController, GroundedParentState> childState)
                {
                    childState.ParentState = this;
                }
            }
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

            Agent.PlayerInfo.CanJump = true;
            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            HorizontalAxisMovement.OvershootDec(true);

            Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);

            SetupFromConfigs(configs);
            TryEnterSubState(configs);
        }
        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.InitialGroundedConfig groundedConfig)
                {
                    if (groundedConfig.IsJumpBuffered)
                    {
                        GD.PushWarning("JUMP BUFFERED");
                        throw new StateInterrupt<JumpState>();
                    }
                    HorizontalAxisMovement.OvershootDec(groundedConfig.DoesDecelerate);
                }
            }
        }

        public override void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction action)
        {
            if (Agent.PlayerInfo.CanJump && actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.JumpAction)
            {
                throw new StateInterrupt<JumpState>(
                    StateConfig.InitialVelocityVector(new Vector2((float)HorizontalAxisMovement.Velocity, Agent.CharacterVariables.JumpSpeed)));
            }

            if (Agent.PlayerInfo.CanDash && actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.DashAction)
            {
                throw new StateInterrupt<DashState>();
            }
            CurrentSubState.OnInputButtonChanged(actionType, action);

        }
        public override void OnInputAxisChanged(InputAxis axis)
        {
            CurrentSubState.OnInputAxisChanged(axis);
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
        public void TryEnterSubState(params IBaseStateConfig[] stateConfigs)
        {
            if (_nextState != null)
            {
                CurrentSubState?.ExitState();
                CurrentSubState = _nextState;
                _nextState = null;
                CurrentSubState.EnterState(stateConfigs);
                _nextConfigs = null;
            }
        }

        public StateExecutionStatus ProcessSubState(StateExecutionStatus processConfig, double delta)
        {
            TryEnterSubState(_nextConfigs);
            return CurrentSubState.ProcessState(processConfig, delta);
        }

        public void ExitSubState()
        {
            TryEnterSubState(_nextConfigs);
            CurrentSubState.ExitState();
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
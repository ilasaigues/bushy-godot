using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class AirParentState : BasePlayerState, IParentState<PlayerController, AirParentState>
    {

        public IChildState<PlayerController, AirParentState> CurrentSubState { get; set; }
        [Export] public BaseState<PlayerController>[] SubStates { get; set; }

        [Export] public Timer CoyoteJumpTimer { get; private set; }

        public AxisMovement XAxisMovement { get; private set; }

        public float VerticalVelocity { get; set; }

        public float TargetHorizontalVelocity { get; set; }

        public bool CanCoyoteJump { get; set; }

        public bool CanFallIntoHedge { get; set; }

        private IChildState<PlayerController, AirParentState> _nextState = null;
        private IBaseStateConfig[] _nextConfigs = null;
        public override bool TryChangeToState(Type type, params IBaseStateConfig[] configs)
        {
            foreach (var subState in SubStates)
            {
                if (subState.GetType() == type && subState is IChildState<PlayerController, AirParentState> childState)
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
                if (genericChild is IChildState<PlayerController, AirParentState> childState)
                {
                    childState.ParentState = this;
                }
            }
            base._Ready();
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
            TryEnterSubState(configs);

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
                    throw new NotImplementedException("Need to implement hedge transition");
                    //Agent.PlayerActionsComponent.EnterHedge(Agent.MovementComponent.HedgeEntered, (float)XAxisMovement.Velocity * Vector2.Right);
                }

                if (VerticalVelocity < 0f && Agent.MovementComponent.IsOnCeiling)
                    VerticalVelocity = 0;

                if (Agent.MovementComponent.IsOnFloor && VerticalVelocity > 0)
                {
                    if (processConfig.CanChangeAnimation)
                    {
                        Agent.AnimationComponent.Play("land");
                    }

                    if (TargetHorizontalVelocity != 0)
                    {
                        throw new StateInterrupt(typeof(WalkState), InitialGrounded(XAxisMovement.HasOvershootDeceleration));
                    }
                    else
                    {
                        throw new StateInterrupt(typeof(IdleGroundedState), InitialGrounded(XAxisMovement.HasOvershootDeceleration));
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
                HandleGravity(delta);
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

        void HandleGravity(double deltaTime)
        {
            VerticalVelocity = Mathf.Min(Agent.CharacterVariables.AirTerminalVelocity, VerticalVelocity + GetGravity() * (float)deltaTime);
        }

        public override void OnInputAxisChanged(InputAxis axis)
        {
            CurrentSubState.OnInputAxisChanged(axis);
        }

        public override void OnInputButtonChanged(InputAction.InputActionType actionType, InputAction Action)
        {
            CurrentSubState.OnInputButtonChanged(actionType, Action);
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
    }
}
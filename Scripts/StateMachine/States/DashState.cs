using System.Linq;
using Godot;
using GodotUtilities;
using static BushyCore.StateConfig;
using static MovementComponent;

namespace BushyCore
{
    public partial class DashState : BaseState<PlayerController>
    {
        private Vector2 constantVelocity;
        private float direction;
        private HedgeNode hedgeNode;

        [Export]
        private Timer DashDurationTimer;
        [Export]
        private Timer DashEndTimer;
        [Export]
        private Timer DashJumpTimer;
        [Export]
        private RayCast2D SlopeRaycast2D;
        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            SetupFromConfigs(configs);
            direction = Agent.MovementInputVector.X != 0
                ? Mathf.Sign(Agent.MovementInputVector.X)
                : Mathf.Sign(Agent.MovementComponent.FacingDirection.X);

            Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
            Agent.PlayerInfo.IsInDashMode = true;
            Agent.PlayerInfo.DashEnabled = false;
            Agent.PlayerInfo.LastDashTime = Time.GetTicksMsec();
            DashDurationTimer.WaitTime = Agent.CharacterVariables.DashTime;
            DashEndTimer.WaitTime = Agent.CharacterVariables.DashExitTime + DashDurationTimer.WaitTime;
            DashJumpTimer.WaitTime = Agent.CharacterVariables.DashJumpWindow;
            DashDurationTimer.Start();
            DashEndTimer.Start();
            DashJumpTimer.Start();
            Agent.MovementComponent.CourseCorrectionEnabled = true;

            StartDash();
            AnimationUpdate();
            base.Agent.CollisionComponent.CallDeferred(
                CharacterCollisionComponent.MethodName.SwitchShape,
                (int)CharacterCollisionComponent.ShapeMode.CILINDER);

            Agent.CollisionComponent.ToggleHedgeCollision(false);
        }

        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    constantVelocity = velocityConfig.Velocity.Normalized() * Agent.CharacterVariables.DashVelocity;
                }
            }
        }

        protected void AnimationUpdate()
        {
            if (DashDurationTimer.TimeLeft > 0)
            {
                Agent.AnimationComponent.Play("dash_start");
            }
            else
            {
                Agent.AnimationComponent.Play("dash_end");
            }
        }


        protected override void ExitStateInternal()
        {
            base.ExitState();

            DashDurationTimer.Stop();
            DashEndTimer.Stop();
            DashJumpTimer.Stop();
            Agent.MovementComponent.CourseCorrectionEnabled = false;

            var exitVelocity = Agent.CharacterVariables.DashExitVelocity;
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2(
                exitVelocity * direction,
                Agent.MovementComponent.Velocities[VelocityType.MainMovement].Y);

            if (!Agent.MovementComponent.IsInHedge)
                Agent.CollisionComponent.ToggleHedgeCollision(true);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (DashEndTimer.TimeLeft == 0)
            {
                EndDash();
            }
            VelocityUpdate();
            CheckHedge();
            return new StateExecutionStatus(StateExecutionResult.Block, MovementLockFlags.All, StateAnimationLevel.Uninterruptible);
        }

        public override bool OnInputButtonChanged(InputAction.InputActionType actionType, InputAction action)
        {
            if (actionType == InputAction.InputActionType.InputPressed && action == InputManager.Instance.JumpAction)
            {
                JumpActionRequested();
            }
            return false;
        }


        public void StartDash()
        {
            constantVelocity.X = Agent.CharacterVariables.DashVelocity;
        }

        public void PreEndDash()
        {
            constantVelocity.X = Agent.CharacterVariables.DashExitVelocity;
        }

        public void EndDash()
        {
            if (Agent.MovementComponent.IsOnFloor)
            {
                if (Agent.MovementInputVector.X != 0)
                {
                    throw new StateInterrupt<WalkState>(new InitialAnimationConfig("turn"));
                }
                else
                {
                    throw new StateInterrupt<IdleGroundedState>(new InitialAnimationConfig("idle"));
                }
            }
            throw new StateInterrupt<FallState>(new InitialAnimationConfig("fall"));
        }

        private void CheckHedge()
        {
            if (!Agent.MovementComponent.IsInHedge) return;

            Agent.AnimationComponent.Play("turn");
            throw new StateInterrupt<HedgeEnteringState>(
                new StateConfig.InitialHedgeConfig(Agent.MovementComponent.HedgeEntered, direction * Vector2.Right));
        }
        protected void VelocityUpdate()
        {

            float verticalComponent = 0;

            // If grounded and non UP ground normal is detected, check whether this angle is for a slope or just a bump 
            if (Agent.MovementComponent.SnappedToFloor && Agent.MovementComponent.FloorAngle != 0)
            {
                var raycastDirection = SlopeRaycast2D.TargetPosition;
                SlopeRaycast2D.TargetPosition = raycastDirection * direction;

                SlopeRaycast2D.ForceRaycastUpdate();

                GodotObject collider = SlopeRaycast2D.GetCollider();

                if (collider != null && collider is TileMap && SlopeRaycast2D.GetCollisionNormal().X != 0)
                {
                    verticalComponent = Mathf.Tan(Agent.MovementComponent.FloorAngle) * constantVelocity.X * direction;
                }

                SlopeRaycast2D.TargetPosition = raycastDirection;
            }

            float horizontalComponent = constantVelocity.X * direction;
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2(horizontalComponent, verticalComponent);
        }

        public void JumpActionRequested()
        {
            if (DashJumpTimer.TimeLeft > 0)
            {
                var jumpVelocity = Agent.MovementComponent.Velocities[VelocityType.MainMovement];
                jumpVelocity.X *= Agent.CharacterVariables.DashJumpSpeed;
                Agent.PlayerInfo.IsInDashMode = true;
                throw new StateInterrupt<JumpState>(new StateConfig.InitialVelocityVectorConfig(jumpVelocity, false, true));
            }
        }

    }
}
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
        private GodotObject hedgeNode;

        [Export]
        private Timer DashDurationTimer;
        [Export]
        private Timer DashEndTimer;
        [Export]
        private Timer DashJumpTimer;
        [Export]
        private RayCast2D SlopeRaycast2D;
        protected override void EnterStateInternal(params IBaseStateConfig[] configs)
        {
            SetupFromConfigs(configs);
            if (constantVelocity.X == 0)
            {
                direction = Agent.PlayerInfo.LookDirection;
                constantVelocity.X = Agent.PlayerInfo.LookDirection * Agent.CharacterVariables.DashVelocity;
            }
            else
            {
                direction = Mathf.Sign(constantVelocity.X);
            }

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
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Dashing, true);
            base.Agent.CollisionComponent.CallDeferred(
                CharacterCollisionComponent.MethodName.SwitchShape,
                (int)CharacterCollisionComponent.ShapeMode.CILINDER);

            Agent.CollisionComponent.ToggleHedgeCollision(false);
        }

        private void SetupFromConfigs(IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is InitialVelocityVectorConfig velocityConfig)
                {
                    constantVelocity = velocityConfig.Velocity.Normalized() * Agent.CharacterVariables.DashVelocity;
                }
            }
        }


        protected override void ExitStateInternal()
        {
            base.ExitState();
            Agent.AnimController.SetCondition(PlayerController.AnimConditions.Dashing, false);

            DashDurationTimer.Stop();
            DashEndTimer.Stop();
            DashJumpTimer.Stop();
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            CheckHedge();
            // 
            if (DashEndTimer.TimeLeft == 0)
            {
                EndDash();
            }
            VelocityUpdate();
            return new StateExecutionStatus(StateExecutionResult.Block, MovementLockFlags.All, StateAnimationLevel.Uninterruptible);
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction action)
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
            Agent.PlayerInfo.IsInDashMode = false;
            Agent.MovementComponent.CourseCorrectionEnabled = false;
            Agent.CollisionComponent.ToggleHedgeCollision(true);
            var exitVelocity = Agent.CharacterVariables.DashExitVelocity;
            Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2(
                exitVelocity * direction,
                Agent.MovementComponent.Velocities[VelocityType.MainMovement].Y); if (Agent.MovementComponent.IsOnFloor)
            {
                ChangeState<WalkState>(true);
            }
            ChangeState<FallState>(true);
        }

        private void CheckHedge()
        {
            if (Agent.MovementComponent.HedgeState == HedgeOverlapState.Outside)
            {
                return;
            }

            ChangeState<HedgeEnteringState>(true,
                    new InitialVelocityVectorConfig(Agent.MovementComponent.CurrentVelocity));
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
                jumpVelocity.X = Mathf.Sign(jumpVelocity.X) * Agent.CharacterVariables.DashJumpSpeed;
                Agent.PlayerInfo.IsInDashMode = true;
                ChangeState<JumpState>(true, new InitialVelocityVectorConfig(jumpVelocity, false, true));
            }
        }

    }
}
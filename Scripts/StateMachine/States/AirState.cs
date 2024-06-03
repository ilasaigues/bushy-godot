using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    [Scene]
    public partial class AirState : BaseMovementState
    {
        private double verticalVelocity;
        private float targetVelocity;
        private bool canCoyoteJump;

        [Node]
        private Timer JumpCoyoteTimer;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            // _directionInputSubscription = _input.DirectionInput.Subscribe(UpdateFacingDirection);
            horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
            verticalVelocity = 0;
            canCoyoteJump = actionsComponent.CanJump;
            
            JumpCoyoteTimer.WaitTime = characterVariables.JumpCoyoteTime;
            JumpCoyoteTimer.Start();

			base.HorizontalAcceleration = characterVariables.AirHorizontalAcceleration;
			base.HorizontalDeceleration = characterVariables.AirHorizontalDeceleration;
			base.HorizontalOvercappedDeceleration = characterVariables.AirHorizontalOvercappedDeceleration;
			base.HorizontalMovementSpeed = characterVariables.AirHorizontalMovementSpeed;
            base.IsConstantHorizontal = false;

            SetupFromConfigs(configs);

			actionsComponent.DashActionStart += DashActionRequested;
			actionsComponent.JumpActionEnd += JumpActionEnded;
        }
        
        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.StartJumpConfig)
                {
                    verticalVelocity = characterVariables.JumpSpeed;
                    actionsComponent.CanJump = false;
                    canCoyoteJump = false;
                }
                if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    verticalVelocity = velocityConfig.Velocity.Y;
                    targetVelocity = velocityConfig.Velocity.X;
                    IsConstantHorizontal = velocityConfig.IsConstant;
                }
            }
        }

        public override void StateExit()
        {
			actionsComponent.DashActionStart -= DashActionRequested;
			actionsComponent.JumpActionEnd -= JumpActionEnded;
            
            base.StateExit();
            // _directionInputSubscription?.Dispose();
        }

        public override void StateUpdateInternal(double delta)
        {
            // if (TryCoyoteJump()) return;
            HandleGravity(delta);
            base.StateUpdateInternal(delta);

            CheckTransitions();
            VelocityUpdate();
            
            if(movementComponent.CurrentVelocity.Y > 0)animationComponent.Play("fall");
        }

        void HandleGravity(double deltaTime)
        {
            verticalVelocity = Mathf.Min(characterVariables.AirTerminalVelocity, verticalVelocity + GetGravity() * (float) deltaTime);
        }

        float GetGravity()
        {
            if (verticalVelocity <= characterVariables.AirSpeedThresholds.Y)
            {
                return characterVariables.AirGravity;
            }
            else if (verticalVelocity <= characterVariables.AirSpeedThresholds.X)
            {
                return characterVariables.AirApexGravity;
            }
            else
            {
                return characterVariables.AirGravity;
            }
        }

        void CheckTransitions() 
        {
            if (canCoyoteJump && actionsComponent.IsJumpRequested)
                actionsComponent.Jump();
                
            if (!movementComponent.IsOnFloor) return;
            
            if (verticalVelocity > 0) 
            {
                actionsComponent.Land();
            }
        }

        protected override void VelocityUpdate()
        {
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) horizontalVelocity * Vector2.Right;
        }

		public void DashActionRequested()
		{
			if (actionsComponent.CanDash)
			{
				RunAndEndState(() => actionsComponent.Dash(this.IntendedDirection));
			}
		} 

        public void OnJumpCoyoteTimerTimeout()
        {
            canCoyoteJump = false;
        }

        public void JumpActionEnded()
        {
            if (verticalVelocity < 0)
                verticalVelocity = characterVariables.JumpShortHopSpeed;
        }
    }
}
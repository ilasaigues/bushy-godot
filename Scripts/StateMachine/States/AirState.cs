using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    [Scene]
    public partial class AirState : BaseState
    {
        private double verticalVelocity;
        private float targetVelocity;
        private bool canCoyoteJump;
        private bool canFallIntoHedge;

		private AxisMovement xAxisMovement;

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

		public override void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationPlayer anim, CharacterCollisionComponent col)
		{
			base.InitState(mc, cv, ac, anim, col);

			this.xAxisMovement = new AxisMovement.Builder()
				.Acc(characterVariables.AirHorizontalAcceleration)
				.Dec(characterVariables.AirHorizontalDeceleration)
				.Speed(characterVariables.AirHorizontalMovementSpeed)
				.OverDec(characterVariables.AirHorizontalOvercappedDeceleration)
				.TurnDec(characterVariables.HorizontalTurnDeceleration)
				.Movement(mc)
				.Direction(() => { return ac.MovementDirection.X; })
				.ColCheck((dir) => { return mc.IsOnWall; })
				.Variables(cv)
				.Build();
		
		}

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            xAxisMovement.SetInitVel(movementComponent.Velocities[VelocityType.MainMovement].X);

            verticalVelocity = 0;
            canFallIntoHedge = false;
            canCoyoteJump = actionsComponent.CanJump;

            JumpCoyoteTimer.WaitTime = characterVariables.JumpCoyoteTime;
            JumpCoyoteTimer.Start();

            xAxisMovement.OvershootDec(true);

            base.collisionComponent.CallDeferred(
                CharacterCollisionComponent.MethodName.SwitchShape, 
                (int) CharacterCollisionComponent.ShapeMode.CILINDER);

            SetupFromConfigs(configs);
			actionsComponent.DashActionStart += DashActionRequested;
			actionsComponent.JumpActionEnd += JumpActionEnded;

            if (canFallIntoHedge)
            {
                collisionComponent.ToggleHedgeCollision(false);
            }
        }
        
        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.StartJumpConfig)
                {   
                    // This path seems unused
                    Debug.WriteLine("Are we ever here?");
                    verticalVelocity = characterVariables.JumpSpeed;
                    actionsComponent.CanJump = false;
                    canCoyoteJump = false;
                }
                if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    verticalVelocity = velocityConfig.Velocity.Y;
                    targetVelocity = velocityConfig.Velocity.X;
                    xAxisMovement.OvershootDec(velocityConfig.DoesDecelerate);
                    canFallIntoHedge = velocityConfig.CanEnterHedge;
                }
            }
        }

        public override void StateExit()
        {
			actionsComponent.DashActionStart -= DashActionRequested;
			actionsComponent.JumpActionEnd -= JumpActionEnded;
            
            if (!movementComponent.IsInHedge)
                collisionComponent.ToggleHedgeCollision(true);

            base.StateExit();
        }

        public override void StateUpdateInternal(double delta)
        {
            HandleGravity(delta);
            base.StateUpdateInternal(delta);

            xAxisMovement.HandleMovement(delta);
            CheckTransitions();
            VelocityUpdate();
            
            if(movementComponent.CurrentVelocity.Y > 0) 
            {
                collisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
            }
        }

        protected override void AnimationUpdate()
        {
            if(movementComponent.CurrentVelocity.Y > 0 && animationComponent.CurrentAnimation != "fall") 
            {
                animationComponent.ClearQueue();
                animationComponent.Play("peak");
                animationComponent.Queue("fall"); 
            }            
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
            
            if (canFallIntoHedge && movementComponent.IsInHedge) 
                actionsComponent.EnterHedge(movementComponent.HedgeEntered, (float) xAxisMovement.Velocity * Vector2.Right); 

            if (verticalVelocity < 0f && movementComponent.IsOnCeiling)
                verticalVelocity = 0;

            if (!movementComponent.IsOnFloor) return;
        
            if (verticalVelocity > 0) 
            {
                animationComponent.Play("land");
                actionsComponent.Land(StateConfig.InitialGrounded(xAxisMovement.HasOvershootDeceleration));
            }
        }

        public void OnHedgeEnter(HedgeNode hedgeNode)
        {
        }

        protected override void VelocityUpdate()
        {
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) xAxisMovement.Velocity * Vector2.Right;
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
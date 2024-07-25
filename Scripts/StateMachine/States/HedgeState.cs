using Godot;
using static MovementComponent;
using System;
using System.Security.AccessControl;
using System.Diagnostics;

namespace BushyCore 
{
	public partial class HedgeState : BaseMovementState
	{
		
		private Vector2 direction;
		private HedgeNode hedgeNode;

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			base.collisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);

			base.HorizontalAcceleration = characterVariables.GroundHorizontalAcceleration;
			base.HorizontalDeceleration = characterVariables.GroundHorizontalDeceleration;
			base.HorizontalMovementSpeed = characterVariables.GroundHorizontalMovementSpeed;
			base.HorizontalOvercappedDeceleration = characterVariables.GroundHorizontalOvercappedDeceleration;
			base.HasOvershootDeceleration = true;

			SetupFromConfigs(configs);

			base.horizontalVelocity = movementComponent.CurrentVelocity.X;
		}

		private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialHedgeConfig hedgeConfig)
				{	
					direction = hedgeConfig.Direction.Normalized();
					hedgeNode = hedgeConfig.Hedge;
				}
			}
		}

        public override void StateExit()
		{
			hedgeNode.ToggleHedgeCollision(true);
			actionsComponent.CanDash = true;
			
			base.StateExit();
		}

		public override void StateUpdateInternal(double delta)
        {
			direction = actionsComponent.MovementDirection;
            base.StateUpdateInternal(delta);

			VelocityUpdate();
        }

        protected override void VelocityUpdate()
        {
			movementComponent.Velocities[VelocityType.MainMovement] = new Vector2((float)horizontalVelocity, direction.Y * 100);
        }

        protected override void AnimationUpdate()
        {

        }

		public void OnHedgeExit(HedgeNode hedgeNode)
		{
			if (!IsActive)
				return;
			
			RunAndEndState(() => {
				hedgeNode.ToggleHedgeCollision(true);
				actionsComponent.Fall();
			});
		}
    }
}




using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
	public partial class HedgeParentState : BaseParentState<PlayerController, HedgeParentState>
	{

		public AxisMovement xAxisMovement;
		public AxisMovement yAxisMovement;

		[Export]
		private Timer JumpBufferTimer;
		[Export]
		private Timer DashBufferTimer;
		[Export]
		private BoolEvent OnEnterHedgeEvent;
		private bool isExitJumpBuffered;
		private bool isExitDashBuffered;

		public Vector2 CurrentVelocity => new((float)xAxisMovement.Velocity, (float)yAxisMovement.Velocity);

		protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			Agent.PlayerInfo.IsInHedge = true;
			OnEnterHedgeEvent?.TriggerEvent(true);
			// Movement axis config
			var builder = new AxisMovement.Builder()
				.Acc(Agent.CharacterVariables.HedgeAcceleration)
				.Dec(Agent.CharacterVariables.HedgeDeceleration)
				.Speed(Agent.CharacterVariables.HedgeMovementSpeed)
				.OverDec(Agent.CharacterVariables.HedgeOvercappedDeceleration)
				.TurnDec(Agent.CharacterVariables.HedgeTurnDeceleration)
				.HasOvershoot(true)
				.Movement(Agent.MovementComponent)
				.Direction(() => { return Agent.MovementInputVector.X; })
				.ColCheck((dir) => { return Agent.MovementComponent.IsOnWall; })
				.Variables(Agent.CharacterVariables);

			xAxisMovement = builder.Build();
			yAxisMovement = builder.Copy()
				.Direction(() => { return Agent.MovementInputVector.Y; })
				.ColCheck((dir) => { return dir > 0 ? Agent.MovementComponent.IsOnFloor : Agent.MovementComponent.IsOnCeiling; })
				.ThresSpeed(Agent.CharacterVariables.MaxHedgeEnterSpeed)
				.Build();


			// Collission config
			Agent.CollisionComponent.ToggleHedgeCollision(false);
			Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);

			// Animation config
			Agent.AnimController.SetCondition(PlayerController.AnimConditions.Hedge, true);

			// Disable gravity
			Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;

			// Value intializations
			// - Buffers
			DashBufferTimer.WaitTime = Agent.CharacterVariables.HedgeDashBufferTime;
			JumpBufferTimer.WaitTime = Agent.CharacterVariables.HedgeJumpBufferTime;
			// - State			
			Agent.PlayerInfo.CanJump = true;
			isExitJumpBuffered = false;
			isExitDashBuffered = false;
			// load configs
			SetupFromConfigs(configs);
		}

		public void SetVelocity(Vector2 velocity)
		{
			xAxisMovement.SetInitVel(velocity.X);
			yAxisMovement.SetInitVel(velocity.Y);
		}

		void SetupFromConfigs(params StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
				{
					SetVelocity(velocityConfig.Velocity);
				}
			}
		}

		protected override void ExitStateInternal()
		{
			Agent.PlayerInfo.IsInHedge = true;

			Agent.AnimController.SetCondition(PlayerController.AnimConditions.Hedge, false);

			Agent.CollisionComponent.ToggleHedgeCollision(true);
			OnEnterHedgeEvent?.TriggerEvent(false);
			Agent.PlayerInfo.DashEnabled = true;
		}

		protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
		{
			yAxisMovement.HandleMovement(delta);
			xAxisMovement.HandleMovement(delta);
			Agent.MovementComponent.Velocities[VelocityType.MainMovement] = CurrentVelocity;
			return ProcessSubState(prevStatus, delta);
		}


		public void OnHedgeExit()
		{
			Agent.CollisionComponent.ToggleHedgeCollision(true);
			Agent.Sprite2DComponent.ForceOrientation(new Vector2(Agent.MovementComponent.CurrentVelocity.X, -10));

			var direction = Agent.MovementComponent.FacingDirection;
			if (isExitDashBuffered && isExitJumpBuffered)
			{
				Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2(Agent.CharacterVariables.DashJumpSpeed * Mathf.Sign(direction.X), 0);
				Agent.PlayerInfo.DashEnabled = false;
				throw StateInterrupt.New<JumpState>(false,
					StateConfig.InitialVelocityVector(
						Agent.MovementComponent.Velocities[VelocityType.MainMovement], false, true));
			}

			if (isExitDashBuffered)
			{
				throw StateInterrupt.New<DashState>(false,
					StateConfig.InitialVelocityVector(new Vector2(Agent.CharacterVariables.DashVelocity * Mathf.Sign(direction.X), 0)));
			}

			if (isExitJumpBuffered)
			{
				throw StateInterrupt.New<JumpState>(false,
					StateConfig.InitialVelocityVector(Agent.MovementComponent.Velocities[VelocityType.MainMovement]));
			}

			throw StateInterrupt.New<FallState>(false, new StateConfig.InitialVelocityVectorConfig(CurrentVelocity));
		}

		private void OnJumpActionCancelled()
		{
			JumpBufferTimer.Stop();
			isExitJumpBuffered = false;
		}
		private void OnJumpBufferTimerEnd()
		{
			isExitJumpBuffered = false;
		}

		private void OnDashActionRequested()
		{
			if (!Agent.PlayerInfo.CanDash)
			{
				return;
			}
			DashBufferTimer.Start();
			isExitDashBuffered = true;
		}

		private void OnDashActionCancelled()
		{
			DashBufferTimer.Stop();
			isExitDashBuffered = false;
		}
		private void OnDashBufferTimerEnd()
		{
			isExitDashBuffered = false;
		}
	}
}
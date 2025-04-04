

using System;
using System.Collections.Generic;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore
{
	public partial class HedgeParentState : BaseParentState<PlayerController, HedgeParentState>
	{

		private HedgeNode HedgeNode;
		public AxisMovement xAxisMovement;
		public AxisMovement yAxisMovement;

		[Export]
		private Timer JumpBufferTimer;
		[Export]
		private Timer DashBufferTimer;
		private bool isExitJumpBuffered;
		private bool isExitDashBuffered;

		public Vector2 Direction;

		protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
		{
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

			xAxisMovement.SetInitVel(Agent.MovementComponent.CurrentVelocity.X);
			yAxisMovement.SetInitVel(Agent.MovementComponent.CurrentVelocity.Y);

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
			// Bind player movement to hedge
			HedgeNode.SubscribeMovementComponent(Agent.MovementComponent);

		}

		void SetupFromConfigs(params StateConfig.IBaseStateConfig[] configs)
		{
			foreach (var config in configs)
			{
				if (config is StateConfig.InitialHedgeConfig hedgeConfig)
				{
					Direction = hedgeConfig.Direction.Normalized();
					HedgeNode = hedgeConfig.Hedge;
				}
			}
		}

		protected override void ExitStateInternal()
		{
			Agent.AnimController.SetCondition(PlayerController.AnimConditions.Hedge, false);

			Agent.CollisionComponent.ToggleHedgeCollision(true);
			Agent.PlayerInfo.DashEnabled = true;
			HedgeNode.UnSubscribeMovementComponent(Agent.MovementComponent);
		}

		protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
		{
			yAxisMovement.HandleMovement(delta);
			xAxisMovement.HandleMovement(delta);
			CheckHedge();
			return ProcessSubState(prevStatus, delta);
		}


		private void CheckHedge()
		{
			if (!Agent.MovementComponent.IsInHedge)
				OnHedgeExit();
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

			throw StateInterrupt.New<FallState>();
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
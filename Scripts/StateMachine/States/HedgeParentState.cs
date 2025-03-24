

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
			Agent.CollisionComponent.ToggleHedgeCollision(false);
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

			Agent.MovementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
			Agent.PlayerInfo.CanJump = true;

			DashBufferTimer.WaitTime = Agent.CharacterVariables.HedgeDashBufferTime;
			JumpBufferTimer.WaitTime = Agent.CharacterVariables.HedgeJumpBufferTime;

			Agent.CollisionComponent.SwitchShape(CharacterCollisionComponent.ShapeMode.RECTANGULAR);
			SetupFromConfigs(configs);

			HedgeNode.SubscribeMovementComponent(Agent.MovementComponent);

			isExitJumpBuffered = false;
			isExitDashBuffered = false;
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
			Agent.CollisionComponent.ToggleHedgeCollision(true);
			Agent.PlayerInfo.DashEnabled = true;
			HedgeNode.UnSubscribeMovementComponent(Agent.MovementComponent);
		}

		public override bool OnAreaChange(Area2D area, bool enter)
		{
			// TODO: Check when area left is hedge, and no other hedges are overlapping
			// Transition to Jump, Dash or Fall states
			return true;
		}

		protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
		{
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

			var direction = Agent.MovementComponent.FacingDirection;
			if (isExitDashBuffered && isExitJumpBuffered)
			{
				Agent.MovementComponent.Velocities[VelocityType.MainMovement] = new Vector2(Agent.CharacterVariables.DashJumpSpeed * Mathf.Sign(direction.X), 0);
				Agent.PlayerInfo.DashEnabled = false;
				throw new StateInterrupt<JumpState>(StateConfig.InitialVelocityVector(
					Agent.MovementComponent.Velocities[VelocityType.MainMovement], false, true));
			}

			if (isExitDashBuffered)
			{
				throw new StateInterrupt<DashState>(StateConfig.InitialVelocityVector(new Vector2(Agent.CharacterVariables.DashVelocity * Mathf.Sign(direction.X), 0)));
			}

			if (isExitJumpBuffered)
			{
				throw new StateInterrupt<JumpState>(
								   StateConfig.InitialVelocityVector(Agent.MovementComponent.Velocities[VelocityType.MainMovement]));
			}

			throw new StateInterrupt<FallState>();
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
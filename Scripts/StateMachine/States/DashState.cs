using BushyCore;
using Godot;
using System;
using static MovementComponent;

public partial class DashState : BaseState
{
	private Vector2 constantVelocity;
	private Vector2 startPoint;
	public override void StateEnter(params StateConfig.IBaseStateConfig[] configs)
	{
		base.StateEnter(configs);
		SetupFromConfigs(configs);

		actionsComponent.CanDash = false;

		startPoint = movementComponent.GlobalPosition;
		movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;
	}
	private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
	{
		foreach (var config in configs)
		{
			if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
			{
				constantVelocity = velocityConfig.Velocity.Normalized() * this.characterVariables.DashVelocity;
			}
		}
	}
    public override void StateUpdateInternal(double delta)
    {
		movementComponent.Velocities[VelocityType.MainMovement] = constantVelocity;
		var travelledDistance = startPoint.DistanceTo(movementComponent.GlobalPosition);

		if (travelledDistance >= this.characterVariables.DashDistance)
		{
			actionsComponent.Fall();
		}
    }
}

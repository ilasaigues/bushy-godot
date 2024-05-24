using BushyCore;
using Godot;
using System;
using static MovementComponent;

public partial class DashState : BaseState
{
	private Vector2 constantVelocity;
	private Vector2 startPoint;
	
	protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
	{
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

    protected override void VelocityUpdate() {}
}

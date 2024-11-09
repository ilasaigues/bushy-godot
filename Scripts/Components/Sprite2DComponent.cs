using Godot;
using System;

public partial class Sprite2DComponent : Sprite2D
{
	[Export]
	private MovementComponent movementComponent;
	[Export]
	private Color OutlineColor;
	

    public override void _Ready()
    {
        movementComponent.CoreographyUpdate += ForceOrientation;
    }
    public override void _ExitTree()
    {
        movementComponent.CoreographyUpdate -= ForceOrientation;
    }
    public override void _Process(double delta)
	{
		CheckSpriteOrientation();
	}

	private void CheckSpriteOrientation()
	{
		var mainVelocity = movementComponent.Velocities[MovementComponent.VelocityType.MainMovement];
		
		if (mainVelocity != Vector2.Zero)
		{
			this.FlipH = movementComponent.FacingDirection.X < 0f;
			return;
		}
	}

	public void ForceOrientation(Vector2 direction)
	{
		this.FlipH = direction.X < 0f;
	}

	public void SetOutline(bool enabled){
		Color NewColor = enabled ? OutlineColor : new Color(0,0,0,0);
		((ShaderMaterial)Material).SetShaderParameter("outline_color", NewColor);
	}
}

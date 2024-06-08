using Godot;
using GodotUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[Tool]
[Scene]
public partial class MovementComponent : Node2D
{
	// Where is the character body facing
	public Vector2 FacingDirection { get; private set; }
	// IsOnFloor references whether the node collider is actually touching the floor
	public bool IsOnFloor { get; private set; } 
	// IsOnRoof references whether the node collider is actually touching the roof
	public bool IsOnCeiling { get; private set; } 
	// IsOnRoof references whether the node collider is next to wall in the facing direction
	public bool IsOnWall { get; private set; } 
	// SnappedToFloor references whether the node collider is within snapping distance from the floor
	public bool SnappedToFloor { get; private set; }
	// Lowest point of collision between the component and the floor
	public float? FloorHeight { get; private set; }
	// Normal between the component and the lowest point of collision with the ground (only relevant if controller is on floor)
	public Vector2 FloorNormal { get; private set; }
	// Angle between Vector2.UP and the Floor normal calculated clockwise
	public float FloorAngle 
	{
		get { 
			return FloorNormal.Y != 0 
				? Mathf.Sign(FloorNormal.X) * Mathf.Atan(Mathf.Abs(FloorNormal.X)/Mathf.Abs(FloorNormal.Y)) 
				: Mathf.Pi/2 * Mathf.Sign(FloorNormal.X); 
		} 
	}
	
	[Node]
	private RayCast2D GroundRayCastL;
	[Node]
	private RayCast2D GroundRayCastR;
	[Node]
	private RayCast2D SideRayCastL;
	[Node]
	private RayCast2D SideRayCastR;

	public enum VelocityType
	{
		MainMovement,
		Gravity,
		InheritedVelocity,
		Dash,
	}
	public Dictionary<VelocityType, Vector2> Velocities = new Dictionary<VelocityType, Vector2>();

	public Vector2 CurrentVelocity => Velocities.Values.AsEnumerable()
		.Append(Vector2.Zero)
		.Aggregate((v1, v2) => v1 + v2);

	public override void _Ready()
	{
		Velocities.Add(VelocityType.MainMovement, Vector2.Zero);
		Velocities.Add(VelocityType.Gravity, Vector2.Zero);
		Velocities.Add(VelocityType.InheritedVelocity, Vector2.Zero);
		Velocities.Add(VelocityType.Dash, Vector2.Zero);	

		GroundRayCastL.Enabled = false;
		GroundRayCastR.Enabled = false;

		FacingDirection = Vector2.Right;
	}
	
	public void UpdateState(CharacterBody2D characterBody2D)
	{
		IsOnFloor = SnappedToFloor = characterBody2D.IsOnFloor();

		if (!IsOnFloor) 
		{
			SnappedToFloor = false;
			FloorHeight = null;
		}

		IsOnCeiling = characterBody2D.IsOnCeiling();
		IsOnWall = characterBody2D.IsOnWall();
		
		FloorNormal = characterBody2D.GetFloorNormal();

		var colCount = characterBody2D.GetSlideCollisionCount();
		if (IsOnFloor && colCount > 0)
		{
			FloorHeight = Enumerable.Range(0, characterBody2D.GetSlideCollisionCount())
				.Select(idx => characterBody2D.GetSlideCollision(idx))
				.Select(col => col.GetPosition().Y)
				.Min();
		}

		// Helps player stay snapped to the floor while going down slopes
		characterBody2D.FloorSnapLength = 5.0f;
		characterBody2D.FloorStopOnSlope = false;
		
		// Not really sure about this. No rays are casted when controller is on the edge of a slope
		if (!IsOnFloor)
		{
			CheckRaycastFloor(GroundRayCastL);
			CheckRaycastFloor(GroundRayCastR);
		}

		FacingDirection = this.Velocities[VelocityType.MainMovement].X == 0f 
			? FacingDirection 
			: this.Velocities[VelocityType.MainMovement].X * Vector2.Right;

	}
	
	private void CheckRaycastFloor(RayCast2D rayCast2D)
	{
		rayCast2D.ForceRaycastUpdate();

		GodotObject collider = rayCast2D.GetCollider();

		if (collider != null && collider is TileMap tileMap) 
		{
			Vector2 point = rayCast2D.GetCollisionPoint();
			if (FloorHeight == null || point.Y < FloorHeight) 
			{
				FloorHeight = point.Y;
				FloorNormal = rayCast2D.GetCollisionNormal();
			}

			SnappedToFloor = true;
		}
	}
	public void Move(CharacterBody2D characterBody2D)
	{
		characterBody2D.Velocity = CurrentVelocity;
		characterBody2D.MoveAndSlide();
	}

	public void SetRaycastPosition(CollisionShape2D collisionShape2D)
	{
		float colliderSizeX = collisionShape2D.Shape.GetRect().Size.X;
		float colliderSizeY = collisionShape2D.Shape.GetRect().Size.Y;

		GroundRayCastL.Position = new Vector2(-colliderSizeX/2, colliderSizeY/2);
		GroundRayCastR.Position = new Vector2(colliderSizeX/2, colliderSizeY/2);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			this.AddToGroup();
			this.WireNodes();
		}
	}

}

using Godot;
using GodotUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[Tool]
[Scene]
public partial class MovementComponent : Node2D
{
	public bool IsOnFloor { get; private set; } 
	public bool SnappedToFloor { get; private set; }
	public Vector2 FloorNormal { get; private set; }
	public float? FloorHeight { get; private set; }
	
	[Node]
	private RayCast2D GroundRayCastL;
	[Node]
	private RayCast2D GroundRayCastR;

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
	}
	
	public void UpdateState(CharacterBody2D characterBody2D)
	{
		IsOnFloor = characterBody2D.IsOnFloor();

		if (!IsOnFloor) 
		{
			SnappedToFloor = false;
			FloorHeight = null;
		}
		
		FloorNormal = characterBody2D.GetFloorNormal();

		var colCount = characterBody2D.GetSlideCollisionCount();
		if (IsOnFloor && colCount > 0)
		{
			FloorHeight = Enumerable.Range(0, characterBody2D.GetSlideCollisionCount())
				.Select(idx => characterBody2D.GetSlideCollision(idx))
				.Select(col => col.GetPosition().Y)
				.Min();
		}

		// Doesnt fix that the CB2D is no longer on floor but does snap the player when falling into a slope
		// characterBody2D.FloorSnapLength = 50.0f;
		// characterBody2D.FloorStopOnSlope = false;
		
		if (!IsOnFloor)
		{
			Debug.WriteLine("raycast check ground");
			CheckRaycastFloor(GroundRayCastL);
			CheckRaycastFloor(GroundRayCastR);
		}

		Debug.WriteLine($"Ground height: {FloorHeight}, normal: {FloorNormal}");
	}
	
	private void CheckRaycastFloor(RayCast2D rayCast2D)
	{
		rayCast2D.ForceRaycastUpdate();

		GodotObject collider = rayCast2D.GetCollider();
		Debug.WriteLine($"collider: {collider}");
		if (collider != null && collider is TileMap tileMap) 
		{
		
			Debug.WriteLine($"collider is tilemap");
			Vector2 point = rayCast2D.GetCollisionPoint();
			Debug.WriteLine($"point: {point}");
			Debug.WriteLine($"Floor height: {FloorHeight}");
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

	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			this.AddToGroup();
			this.WireNodes();
		}
	}

}

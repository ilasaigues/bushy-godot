using BushyCore;
using Godot;
using GodotUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
	public bool IsOnEdge { get { return _raysOnFloor == 1; } }
	public HedgeNode HedgeEntered;
	public bool IsInHedge { get { return HedgeEntered != null && _raysOnHedge > 1; }}

	private int _raysOnFloor;
	private int _raysOnHedge;
	

	public bool CourseCorrectionEnabled;
	
	[Node]
	private RayCast2D GroundRayCastL;
	[Node]
	private RayCast2D GroundRayCastR;

	[Node]
	private RayCast2D CourseCorrectRayXu;
	[Node]
	private RayCast2D CourseCorrectRayXd;
	[Node]
	private RayCast2D CourseCorrectRayYl;
	[Node]
	private RayCast2D CourseCorrectRayYr;


	[Export]
	private CollisionShape2D CollisionComponent;
	private PlayerController parentController;

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
		CourseCorrectRayXd.Enabled = false;
		CourseCorrectRayXu.Enabled = false;
		CourseCorrectRayYl.Enabled = false;
		CourseCorrectRayYr.Enabled = false;

		FacingDirection = Vector2.Right;

		SetRaycastPosition();
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
		
		this._raysOnFloor = 0;
		this._raysOnHedge = 0;

		CheckRaycastFloor(GroundRayCastL);
		CheckRaycastFloor(GroundRayCastR);

		FacingDirection = this.Velocities[VelocityType.MainMovement].X == 0f 
			? FacingDirection 
			: this.Velocities[VelocityType.MainMovement].X * Vector2.Right;
	}
	
	private void CheckRaycastFloor(RayCast2D rayCast2D)
	{
		rayCast2D.ForceRaycastUpdate();

		GodotObject collider = rayCast2D.GetCollider();

		if (collider != null) 
		{
			Vector2 point = rayCast2D.GetCollisionPoint();
			if (FloorHeight == null || point.Y < FloorHeight) 
			{
				FloorHeight = point.Y;
				FloorNormal = rayCast2D.GetCollisionNormal();
			}

			SnappedToFloor = true;
			this._raysOnFloor++;

			if (((Node2D) collider).GetParent() is HedgeNode)
			{	
				_raysOnHedge++;
			}
		}
	}
	public void Move(CharacterBody2D characterBody2D)
	{
		characterBody2D.Velocity = CurrentVelocity;
		ApplyCourseCorrection(characterBody2D);
	
		characterBody2D.MoveAndSlide();
	}
	public void SetParentController(PlayerController val) { this.parentController = val; }

	private void ApplyCourseCorrection(CharacterBody2D characterBody2D)
	{
		if (!CourseCorrectionEnabled) return; 

		var colSize = this.CollisionComponent.Shape.GetRect().Size;
		var extent = Mathf.Sign(this.FacingDirection.X) * colSize.X/2;
		
		// Lower corner check
		this.CourseCorrectRayXd.TargetPosition = 20 * Vector2.Right * Mathf.Sign(this.FacingDirection.X);
		this.CourseCorrectRayXu.TargetPosition = 20 * Vector2.Right * Mathf.Sign(this.FacingDirection.X);
		
		this.CourseCorrectRayXu.Position = new Vector2(extent, -3f);
		this.CourseCorrectRayXd.Position = new Vector2(extent, colSize.Y/2);

		this.CourseCorrectRayXu.ForceRaycastUpdate();
		this.CourseCorrectRayXd.ForceRaycastUpdate();

		var colliderUp = this.CourseCorrectRayXu.GetCollider();
		var colliderDown = this.CourseCorrectRayXd.GetCollider();

		bool upcomingCorner = colliderUp is not TileMap && colliderDown is TileMap;
		upcomingCorner &= (this.FacingDirection.Normalized() + this.CourseCorrectRayXd.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			parentController.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y - 3f);
			return;
		}
		
		// Upper corner check
		this.CourseCorrectRayXu.Position = new Vector2(extent, -colSize.Y/2);
		this.CourseCorrectRayXd.Position = new Vector2(extent, 3f);

		this.CourseCorrectRayXu.ForceRaycastUpdate();
		this.CourseCorrectRayXd.ForceRaycastUpdate();

		colliderUp = this.CourseCorrectRayXu.GetCollider();
		colliderDown = this.CourseCorrectRayXd.GetCollider();

		upcomingCorner = colliderUp is TileMap && colliderDown is not TileMap;
		upcomingCorner &= (this.FacingDirection.Normalized() + this.CourseCorrectRayXu.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			parentController.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + 3f);
			return;
		}
	}

	public void SetRaycastPosition()
	{
		float colliderSizeX = CollisionComponent.Shape.GetRect().Size.X;
		float colliderSizeY = CollisionComponent.Shape.GetRect().Size.Y;

		GroundRayCastL.Position = new Vector2(-colliderSizeX/2, colliderSizeY/2);
		GroundRayCastR.Position = new Vector2(colliderSizeX/2, colliderSizeY/2);

		CourseCorrectRayXu.Position = new Vector2(colliderSizeX/2,0);
		CourseCorrectRayXd.Position = new Vector2(colliderSizeX/2,0);
		CourseCorrectRayYl.Position = new Vector2(0,colliderSizeY/2);
		CourseCorrectRayYr.Position = new Vector2(0,colliderSizeY/2);
	}

	public void OnHedgeEnter(HedgeNode hedgeNode)
	{
		this.HedgeEntered = hedgeNode;
	}
	
	public void OnHedgeExit(HedgeNode hedgeNode)
	{
		this.HedgeEntered = null;
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

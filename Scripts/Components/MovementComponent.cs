using BushyCore;
using Godot;
using GodotUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

[Scene]
public partial class MovementComponent : Node2D
{
	public enum HedgeOverlapState
	{
		Outside,
		Partial,
		Complete,
	}

	// IsOnFloor references whether the node collider is actually touching the floor
	public bool IsOnFloor { get; set; }
	// FloorHeightCheckEnabled enables setting the last known height at which the player touched the ground
	public bool FloorHeightCheckEnabled { get; set; }
	// IsOnRoof references whether the node collider is actually touching the roof
	public bool IsOnCeiling { get; private set; }
	// IsOnRoof references whether the node collider is next to wall in the facing direction
	public bool IsOnWall { get; private set; }
	/// <summary>
	/// Current ovelap state against hedge objects. 0 = not overlapping, 1 = partial overlapping, 2 = fully overlapped
	/// </summary>
	public HedgeOverlapState HedgeState { get; private set; }
	public Vector2 InsideHedgeDirection { get; private set; }
	// SnappedToFloor references whether the node collider is within snapping distance from the floor
	public bool SnappedToFloor { get; private set; }
	// Lowest point of collision between the component and the floor
	public float? FloorHeight { get; private set; }
	// Normal between the component and the lowest point of collision with the ground (only relevant if controller is on floor)
	public Vector2 FloorNormal { get; private set; }
	// Angle between Vector2.UP and the Floor normal calculated clockwise
	public float FloorAngle
	{
		get
		{
			return FloorNormal.Y != 0
				? Mathf.Sign(FloorNormal.X) * Mathf.Atan(Mathf.Abs(FloorNormal.X) / Mathf.Abs(FloorNormal.Y))
				: Mathf.Pi / 2 * Mathf.Sign(FloorNormal.X);
		}
	}
	public bool IsOnEdge => _raysOnFloor == 1;
	public bool IsStandingOnHedge => _raysOnHedge > 0;
	private int _raysOnHedge = 0;

	public bool CanDropFromPlatform = false;

	private bool _isCoreography;
	private int _raysOnFloor;
	private int _raysOnWall;
	public Vector2 OutsideHedgeDirection;

	private float _lastPlatformHeight;

	public Vector2 RealPositionChange { get; private set; } = Vector2.Zero;
	private Vector2 _previousPosition;

	public bool CourseCorrectionEnabled;

	[Node]
	private RayCast2D GroundRayCastL;
	[Node]
	private RayCast2D GroundRayCastR;
	[Node]
	private RayCast2D CeilRayCastL;
	[Node]
	private RayCast2D CeilRayCastR;

	[Node]
	private RayCast2D CourseCorrectRayXu;
	[Node]
	private RayCast2D CourseCorrectRayXd;
	[Node]
	private RayCast2D CourseCorrectRayYl;
	[Node]
	private RayCast2D CourseCorrectRayYr;
	[Node]
	private RayCast2D ReusableRay;

	[Export]
	private CollisionShape2D CollisionComponent;
	public PlayerController ParentController { get; private set; }

	public enum VelocityType
	{
		MainMovement,
		Gravity,
		InheritedVelocity,
		Dash,
		Locked,
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
		CeilRayCastL.Enabled = false;
		CeilRayCastR.Enabled = false;
		CourseCorrectRayXd.Enabled = false;
		CourseCorrectRayXu.Enabled = false;
		CourseCorrectRayYl.Enabled = false;
		CourseCorrectRayYr.Enabled = false;

		_previousPosition = GlobalPosition;

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

		_raysOnFloor = 0;
		_raysOnWall = 0;
		_raysOnHedge = 0;

		CheckHedge();

		if (HedgeState == HedgeOverlapState.Outside)
		{
			CheckRaycastFloor(GroundRayCastL);
			CheckRaycastFloor(GroundRayCastR);
		}
		CanDropFromPlatform = (CheckRaycastPlatform(GroundRayCastL) || CheckRaycastPlatform(GroundRayCastR)) && _raysOnFloor == 0;

		_isCoreography = Velocities[VelocityType.MainMovement] == Vector2.Zero && _isCoreography;
	}

	private void CheckRaycastFloor(RayCast2D rayCast2D)
	{
		rayCast2D.ForceRaycastUpdate();

		GodotObject collider = rayCast2D.GetCollider();

		if (collider != null)
		{

			Vector2 point = rayCast2D.GetCollisionPoint();
			if (FloorHeight == null || rayCast2D.GetCollisionNormal() == Vector2.Up || point.Y < FloorHeight)
			{
				FloorHeight = point.Y;
				FloorNormal = rayCast2D.GetCollisionNormal();
			}

			SnappedToFloor = true;
			_raysOnFloor++;
		}
	}
	private void CheckHedge()
	{
		var bounds = ParentController.CollisionComponent.Shape.GetRect();
		var tl = bounds.Position;
		var tr = new Vector2(bounds.End.X, tl.Y);
		var bl = new Vector2(tl.X, bounds.End.Y);
		var br = bounds.End;
		var corners = new Vector2[] { tl, tr, bl, br };
		var overlapCount = 0;
		InsideHedgeDirection = Vector2.Zero;

		ReusableRay.CollisionMask = 1 << 2;
		foreach (var corner in corners)
		{
			ReusableRay.Position = corner;
			ReusableRay.TargetPosition = -corner / 2;
			ReusableRay.ForceRaycastUpdate();
			if (ReusableRay.IsColliding())
			{
				overlapCount++;
				InsideHedgeDirection += corner / 2;
			}
		}
		ReusableRay.Position = bl;
		ReusableRay.TargetPosition = bl + Vector2.Down;
		ReusableRay.ForceRaycastUpdate();
		if (ReusableRay.IsColliding())
		{
			_raysOnHedge++;
		}
		ReusableRay.Position = br;
		ReusableRay.TargetPosition = br + Vector2.Down;
		ReusableRay.ForceRaycastUpdate();
		if (ReusableRay.IsColliding())
		{
			_raysOnHedge++;
		}

		InsideHedgeDirection = InsideHedgeDirection.Normalized();
		HedgeState = overlapCount == 0 ? HedgeOverlapState.Outside : overlapCount == 4 ? HedgeOverlapState.Complete : HedgeOverlapState.Partial;
	}

	private bool CheckRaycastPlatform(RayCast2D groundCast)
	{
		var prevMask = groundCast.CollisionMask;
		groundCast.CollisionMask = 1 << 5; //platfrom layer
		groundCast.ForceRaycastUpdate();
		groundCast.CollisionMask = prevMask;
		return groundCast.IsColliding();
	}

	public void Move(CharacterBody2D characterBody2D)
	{
		// Debug.WriteLine("MOVE");
		// Debug.WriteLine(this.Velocities[VelocityType.MainMovement]);
		characterBody2D.Velocity = CurrentVelocity;
		ApplyCourseCorrection(characterBody2D);
		characterBody2D.MoveAndSlide();
		float y = ParentController.GlobalPosition.Y;
		float x = IsOnWall ? ParentController.GlobalPosition.Round().X : ParentController.GlobalPosition.X;
		ParentController.GlobalPosition = new Vector2(x, y);
	}
	public void SetParentController(PlayerController val) { ParentController = val; }

	private void ApplyCourseCorrection(CharacterBody2D characterBody2D)
	{
		if (!CourseCorrectionEnabled) return;

		var colSize = CollisionComponent.Shape.GetRect().Size;
		var extent = Mathf.Sign(ParentController.PlayerInfo.LookDirection) * colSize.X / 2;

		// Lower corner check
		CourseCorrectRayXd.TargetPosition = 20 * Vector2.Right * Mathf.Sign(ParentController.PlayerInfo.LookDirection);
		CourseCorrectRayXu.TargetPosition = 20 * Vector2.Right * Mathf.Sign(ParentController.PlayerInfo.LookDirection);

		CourseCorrectRayXu.Position = new Vector2(extent, -3f);
		CourseCorrectRayXd.Position = new Vector2(extent, colSize.Y / 2);

		CourseCorrectRayXu.ForceRaycastUpdate();
		CourseCorrectRayXd.ForceRaycastUpdate();

		var colliderUp = CourseCorrectRayXu.GetCollider();
		var colliderDown = CourseCorrectRayXd.GetCollider();

		bool upcomingCorner = colliderUp is not TileMap && colliderDown is TileMap;
		upcomingCorner &= (ParentController.PlayerInfo.LookDirection * Vector2.Right + CourseCorrectRayXd.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			ParentController.GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - 3f);
			return;
		}

		// Upper corner check
		CourseCorrectRayXu.Position = new Vector2(extent, -colSize.Y / 2);
		CourseCorrectRayXd.Position = new Vector2(extent, 3f);

		CourseCorrectRayXu.ForceRaycastUpdate();
		CourseCorrectRayXd.ForceRaycastUpdate();

		colliderUp = CourseCorrectRayXu.GetCollider();
		colliderDown = CourseCorrectRayXd.GetCollider();

		upcomingCorner = colliderUp is TileMap && colliderDown is not TileMap;
		upcomingCorner &= (ParentController.PlayerInfo.LookDirection * Vector2.Right + CourseCorrectRayXu.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			ParentController.GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y + 3f);
			return;
		}
	}

	public void SetRaycastPosition()
	{
		if (Engine.IsEditorHint())
			return;
		float colliderSizeX = CollisionComponent.Shape.GetRect().Size.X;
		float colliderSizeY = CollisionComponent.Shape.GetRect().Size.Y;

		GroundRayCastL.Position = new Vector2(-colliderSizeX / 2, colliderSizeY / 2);
		GroundRayCastR.Position = new Vector2(colliderSizeX / 2, colliderSizeY / 2);

		CeilRayCastR.Position = new Vector2(colliderSizeX / 2, -colliderSizeY / 2);
		CeilRayCastL.Position = new Vector2(-colliderSizeX / 2, -colliderSizeY / 2);

		CourseCorrectRayXu.Position = new Vector2(colliderSizeX / 2, 0);
		CourseCorrectRayXd.Position = new Vector2(colliderSizeX / 2, 0);
		CourseCorrectRayYl.Position = new Vector2(0, colliderSizeY / 2);
		CourseCorrectRayYr.Position = new Vector2(0, colliderSizeY / 2);
	}


	public void StartCoreography()
	{
		_isCoreography = true;
	}

	public void CoreographFaceDirection(Vector2 direction)
	{
		if (_isCoreography)
			EmitSignal(SignalName.CoreographyUpdate, direction);
	}

	[Signal]
	public delegate void CoreographyUpdateEventHandler(Vector2 direction);
	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated)
		{
			this.AddToGroup();
			WireNodes();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		RealPositionChange = (ParentController.GlobalPosition - _previousPosition) / (float)delta;
		_previousPosition = ParentController.GlobalPosition;
	}
}

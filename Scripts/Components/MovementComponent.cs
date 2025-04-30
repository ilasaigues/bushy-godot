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
	public bool IsOnFloor { get; set; }
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
		get
		{
			return FloorNormal.Y != 0
				? Mathf.Sign(FloorNormal.X) * Mathf.Atan(Mathf.Abs(FloorNormal.X) / Mathf.Abs(FloorNormal.Y))
				: Mathf.Pi / 2 * Mathf.Sign(FloorNormal.X);
		}
	}
	public bool IsOnEdge => _raysOnFloor == 1;
	public GodotObject OverlappedHedge;

	public bool ShouldEnterHedge => OverlappedHedge != null && (_raysOnHedge + _raysOnWall) >= 2;
	public bool ShouldExitHedge => _raysOnWall == 0 && _raysOnHedge < 4;

	public bool CanDropFromPlatform = false;

	private bool _isCoreography;
	private int _raysOnFloor;
	private int _raysOnHedge;
	private int _raysOnWall;
	public Vector2 OutsideHedgeDirection;
	public Vector2 InsideHedgeDirection;

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


	[Export]
	private CollisionShape2D CollisionComponent;
	private PlayerController parentController;

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

		FacingDirection = Vector2.Right;
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
		_raysOnHedge = 0;
		_raysOnWall = 0;

		CheckRaycastFloor(GroundRayCastL);
		CheckRaycastFloor(GroundRayCastR);

		OutsideHedgeDirection = InsideHedgeDirection = Vector2.Zero;

		CheckRaycastHedge(GroundRayCastR, -Vector2.Up + Vector2.Right);
		CheckRaycastHedge(CeilRayCastR, Vector2.Up + Vector2.Right);
		CheckRaycastHedge(GroundRayCastL, -Vector2.Up - Vector2.Right);
		CheckRaycastHedge(CeilRayCastL, Vector2.Up - Vector2.Right);

		if (_raysOnHedge < 2)
		{
			OverlappedHedge = null;
		}

		CanDropFromPlatform = CheckRaycastPlatform(GroundRayCastL) && CheckRaycastPlatform(GroundRayCastR);

		FacingDirection = Velocities[VelocityType.MainMovement].X == 0f
			? FacingDirection
			: Velocities[VelocityType.MainMovement].X * Vector2.Right;

		_isCoreography = Velocities[VelocityType.MainMovement] == Vector2.Zero && _isCoreography;
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
			_raysOnFloor++;
		}
	}
	private void CheckRaycastHedge(RayCast2D rayCast2D, Vector2 direction)
	{
		var prevPosition = rayCast2D.TargetPosition;
		rayCast2D.Position = direction.Normalized() * prevPosition.Length();
		var prevMask = rayCast2D.CollisionMask;
		GodotObject collider = null;
		// if inside the bush, check check for walls and count them as "in the hedge"
		if (OverlappedHedge != null)
		{
			rayCast2D.CollisionMask = 1 << 1 | 1 << 3;
			rayCast2D.ForceRaycastUpdate();
			collider = rayCast2D.GetCollider();
			if (collider != null)
			{
				_raysOnWall++;
				OutsideHedgeDirection += direction;
				InsideHedgeDirection -= direction;
			}
		}

		if (collider == null) // if we do not hit a wall, proceed as usual
		{
			// check for in/out of hedge
			rayCast2D.CollisionMask = 1 << 2;
			rayCast2D.ForceRaycastUpdate();
			collider = rayCast2D.GetCollider();

			if (collider != null)
			{
				_raysOnHedge++;
				InsideHedgeDirection += direction;

				if (_raysOnHedge + _raysOnWall >= 2)
				{
					OverlappedHedge = collider;
				}
			}
			else
			{
				OutsideHedgeDirection += direction;
			}
		}
		rayCast2D.CollisionMask = prevMask;
		rayCast2D.TargetPosition = prevPosition;
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
		float y = parentController.GlobalPosition.Y;
		float x = IsOnWall ? parentController.GlobalPosition.Round().X : parentController.GlobalPosition.X;
		parentController.GlobalPosition = new Vector2(x, y);
	}
	public void SetParentController(PlayerController val) { parentController = val; }

	private void ApplyCourseCorrection(CharacterBody2D characterBody2D)
	{
		if (!CourseCorrectionEnabled) return;

		var colSize = CollisionComponent.Shape.GetRect().Size;
		var extent = Mathf.Sign(FacingDirection.X) * colSize.X / 2;

		// Lower corner check
		CourseCorrectRayXd.TargetPosition = 20 * Vector2.Right * Mathf.Sign(FacingDirection.X);
		CourseCorrectRayXu.TargetPosition = 20 * Vector2.Right * Mathf.Sign(FacingDirection.X);

		CourseCorrectRayXu.Position = new Vector2(extent, -3f);
		CourseCorrectRayXd.Position = new Vector2(extent, colSize.Y / 2);

		CourseCorrectRayXu.ForceRaycastUpdate();
		CourseCorrectRayXd.ForceRaycastUpdate();

		var colliderUp = CourseCorrectRayXu.GetCollider();
		var colliderDown = CourseCorrectRayXd.GetCollider();

		bool upcomingCorner = colliderUp is not TileMap && colliderDown is TileMap;
		upcomingCorner &= (FacingDirection.Normalized() + CourseCorrectRayXd.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			parentController.GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - 3f);
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
		upcomingCorner &= (FacingDirection.Normalized() + CourseCorrectRayXu.GetCollisionNormal()).Length() < 0.00001f;

		if (upcomingCorner)
		{
			parentController.GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y + 3f);
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
		RealPositionChange = GlobalPosition - _previousPosition;
		_previousPosition = GlobalPosition;
	}
}

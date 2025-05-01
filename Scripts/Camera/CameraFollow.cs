using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using BushyCore;
using Godot;

public partial class CameraFollow : Camera2D
{
    [Export] private CameraTargetBehaviour _targetBehaviour = new PlayerCameraTargetBehaviour();
    [Export] private Node2D _targetNode;

    enum SnappingAnchor
    {
        NONE,
        RIGHT,
        LEFT,
    }

    [ExportCategory("Debug")]
    [Export] private bool _drawDebugLines = true;

    [ExportCategory("Horizontal")]
    [Export] public float _horizontalAnchors = 50;
    [Export] public float _horizontalThresholds = 70;

    [Export] public float _horizontalSnapSpeed = 2;

    [Export(PropertyHint.Range, "0,1")]
    public float _horizontalSmoothing;

    [ExportCategory("Vertical")]
    [Export] public float _bottomThreshold = 20;
    [Export] public float _topThreshold = -50;
    [Export] public float _verticalOffset;
    [Export(PropertyHint.Range, "0,1")]
    public float _verticalSmoothing;
    [ExportCategory("Free Follow")]
    [Export] private BoolEvent _freeFollowToggleEvent;

    [Export(PropertyHint.Range, "0,1")]
    public float _freeFollowSmoothing;
    private bool _freeFollow = false;




    private SnappingAnchor _snappingAnchor = SnappingAnchor.RIGHT;

    private int _lastDirection;

    private float _lastFloorHeight = 0.0f;
    private Node2D midTarget;
    private Node2D overrideTarget;

    private Vector2 _targetPosition;

    public override void _EnterTree()
    {
        _targetPosition = GlobalPosition;
        _targetBehaviour.TargetNode = _targetNode;
        _targetBehaviour.Camera = this;
        if (_freeFollowToggleEvent != null)
        {
            _freeFollowToggleEvent.OnEventTriggered += SetFreeFollow;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (_freeFollowToggleEvent != null)
        {
            _freeFollowToggleEvent.OnEventTriggered -= SetFreeFollow;
        }
    }

    private void SetFreeFollow(bool freeFollow)
    {
        _freeFollow = freeFollow;
    }

    public override void _PhysicsProcess(double delta)
    {

        if (_targetNode == null || _targetBehaviour == null)
        {
            return;
        }

        Vector2 charPos = _targetNode.GlobalPosition;
        Vector2 charVelocity = _targetBehaviour.GetFrameVelocity();


        if (_freeFollow)
        {
            _targetPosition = charPos;
            GlobalPosition = GlobalPosition.Lerp(_targetPosition, _freeFollowSmoothing);
            return;
        }


        if (Mathf.Abs(charVelocity.X) < 0.1)
        {
            charVelocity.X = 0;
        }

        // HANDLE HORIZONTAL
        float leftThreshold = charPos.X - _horizontalThresholds;
        float rightThreshold = charPos.X + _horizontalThresholds;
        float leftAnchor = charPos.X - _horizontalAnchors;
        float rightAnchor = charPos.X + _horizontalAnchors;

        if (GlobalPosition.X <= leftThreshold)
            _snappingAnchor = SnappingAnchor.RIGHT;
        if (GlobalPosition.X >= rightThreshold)
            _snappingAnchor = SnappingAnchor.LEFT;

        float horizontalVelocity = 0;

        if (!(_snappingAnchor == SnappingAnchor.RIGHT && charVelocity.X > 0) && !(_snappingAnchor == SnappingAnchor.LEFT && charVelocity.X < 0))
        {
            _snappingAnchor = SnappingAnchor.NONE;
        }

        if (_snappingAnchor == SnappingAnchor.RIGHT && charVelocity.X > 0)
        {
            horizontalVelocity = charVelocity.X * _horizontalSnapSpeed * (float)delta;

            if (GlobalPosition.X + horizontalVelocity > rightAnchor)
            {
                horizontalVelocity = rightAnchor - GlobalPosition.X;
            }
        }
        if (_snappingAnchor == SnappingAnchor.LEFT && charVelocity.X < 0)
        {
            horizontalVelocity = charVelocity.X * _horizontalSnapSpeed * (float)delta;
            if (GlobalPosition.X + horizontalVelocity < leftAnchor)
            {
                horizontalVelocity = leftAnchor - GlobalPosition.X;
            }
        }

        // HANDLE VERTICAL
        var topThreshold = charPos.Y + _topThreshold;
        var bottomThreshold = charPos.Y + _bottomThreshold;

        // if we're on the floor:
        if (_targetNode is PlayerController pc)
        {
            if (pc.MovementComponent.IsOnFloor)
            {
                _lastFloorHeight = pc.GlobalPosition.Y;
                _targetPosition.Y = _lastFloorHeight + _verticalOffset;
            }
            else
            {
                if (GlobalPosition.Y < topThreshold || GlobalPosition.Y > bottomThreshold)
                {
                    _targetPosition.Y = pc.GlobalPosition.Y + _verticalOffset;
                }
            }
        }


        _targetPosition += Vector2.Right * horizontalVelocity;

        if (midTarget != null)
        {
            _targetPosition = (_targetPosition + midTarget.GlobalPosition) / 2;
        }
        else if (overrideTarget != null)
        {
            _targetPosition = overrideTarget.GlobalPosition;
        }


        GlobalPosition = new Vector2(
            Mathf.Lerp(GlobalPosition.X, _targetPosition.X, _horizontalSmoothing),
             Mathf.Lerp(GlobalPosition.Y, _targetPosition.Y, _verticalSmoothing)
            );
        QueueRedraw();
    }


    public override void _Draw()
    {
        if (!_drawDebugLines)
        {
            return;
        }
        var white = new Color(1, 1, 1);
        var red = new Color(1, 0, 0);
        var green = new Color(0, 1, 0);
        var yellow = new Color(1, 1, 0);


        var offset = _targetNode.GlobalPosition - GlobalPosition;
        DrawLine(Vector2.Up * 20, Vector2.Down * 20, white);
        DrawLine(Vector2.Left * 20, Vector2.Right * 20, white);
        DrawLine(offset + new Vector2(-_horizontalAnchors, 100), offset + new Vector2(-_horizontalAnchors, -100), _snappingAnchor == SnappingAnchor.LEFT ? green : red);
        DrawLine(offset + new Vector2(_horizontalAnchors, 100), offset + new Vector2(_horizontalAnchors, -100), _snappingAnchor == SnappingAnchor.RIGHT ? green : red);
        DrawDashedLine(offset + new Vector2(-_horizontalThresholds, 75), offset + new Vector2(-_horizontalThresholds, -75), yellow);
        DrawDashedLine(offset + new Vector2(_horizontalThresholds, 75), offset + new Vector2(_horizontalThresholds, -75), yellow);

        DrawDashedLine(offset + new Vector2(-75, _lastFloorHeight - _targetNode.GlobalPosition.Y), offset + new Vector2(75, _lastFloorHeight - _targetNode.GlobalPosition.Y), yellow);

        DrawLine(offset + new Vector2(-100, _bottomThreshold), offset + new Vector2(100, _bottomThreshold), yellow);
        DrawLine(offset + new Vector2(-100, _topThreshold), offset + new Vector2(100, _topThreshold), yellow);
    }

    // Call this function to set the target
    public void SetTargetBehaviour(CameraTargetBehaviour newTargetBehaviour)
    {
        _targetBehaviour = newTargetBehaviour;
    }

    // Call this function to set secondary targets
    public void SetMidTarget(Node2D newMidTarget)
    {
        midTarget = newMidTarget;
    }

    public void SetOverrideTarget(Node2D newOverrideTarget)
    {
        overrideTarget = newOverrideTarget;
    }

    // Call this function to update the floor height
    public void UpdateFloorHeight(float newHeight)
    {
        _lastFloorHeight = newHeight;
    }

}
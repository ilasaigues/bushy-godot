using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;

public partial class CameraFollow : Camera2D
{
    // Export variables for configuration
    [Export] public float DirectionOffset = 100.0f;
    [Export] public Vector2 LookAheadFactor;
    [Export] public float DampingSpeed = 0.1f;
    [Export] public float DeadzoneSize = 50.0f;
    [Export] public float VerticalLockThresholdAbove = 20.0f;
    [Export] public float VerticalLockThresholdBelow = 20.0f;
    [Export] public CameraTargetBehaviour _targetBehaviour;
    [Export] private Node2D _targetNode;
    private Node2D midTarget;
    private Node2D overrideTarget;
    private float lastFloorHeight = 0.0f;

    private Vector2 _targetPosition;

    private int lastDirection;

    public override void _EnterTree()
    {
        _targetBehaviour.TargetNode = _targetNode;
        _targetBehaviour.Camera = this;
    }

    public override void _PhysicsProcess(double delta)
    {

        if (_targetBehaviour == null || _targetBehaviour.TargetNode == null)
            return;

        Vector2 charPos = _targetNode.GlobalPosition;

        Vector2 velocity = _targetBehaviour.GetFrameVelocity();

        lastDirection = velocity.X > 0 ? 1 : (velocity.X < 0 ? -1 : lastDirection);

        // HORIZONTAL FOLLOWING
        float cameraX = GlobalPosition.X;
        float leftDeadzone = cameraX - DeadzoneSize / 2f;
        float rightDeadzone = cameraX + DeadzoneSize / 2f;

        float targetX = cameraX;

        if (charPos.X < leftDeadzone)
            targetX = charPos.X + DirectionOffset * lastDirection;
        else if (charPos.X > rightDeadzone)
            targetX = charPos.X + DirectionOffset * lastDirection;

        targetX += velocity.X * LookAheadFactor.X;

        // VERTICAL FOLLOWING
        float targetY;

        bool aboveUpperThreshold = charPos.Y < lastFloorHeight - VerticalLockThresholdAbove;
        bool belowLowerThreshold = charPos.Y > lastFloorHeight + VerticalLockThresholdBelow;

        if (_targetBehaviour.ShouldUpdateLastHeight)
        {
            UpdateFloorHeight(charPos.Y);
        }

        if (belowLowerThreshold || aboveUpperThreshold) // outside threshold, we follow fixed
        {
            targetY = charPos.Y + velocity.Y * LookAheadFactor.Y;
        }
        else
        {
            targetY = lastFloorHeight;
        }

        _targetPosition = new Vector2(targetX, targetY);
        // Midpoint secondary target
        if (midTarget != null)
        {
            _targetPosition += (midTarget.GlobalPosition - Offset) / 2;
        }

        // Override secondary target
        if (overrideTarget != null)
        {
            _targetPosition = overrideTarget.GlobalPosition - Offset;
        }

        // SMOOTH FOLLOW
        float lerpFactor = Mathf.Clamp((float)delta * DampingSpeed, 0f, 1f);
        GlobalPosition = GlobalPosition.Lerp(_targetPosition + Offset, lerpFactor);
    }


    public override void _Draw()
    {
        DrawHorizontalLine(lastFloorHeight);
        DrawHorizontalLine(lastFloorHeight - VerticalLockThresholdAbove);
        DrawHorizontalLine(lastFloorHeight + VerticalLockThresholdBelow);
    }

    void DrawHorizontalLine(float height)
    {
        DrawLine(new Vector2(-10000, height) - GlobalPosition, new Vector2(-10000, height) - GlobalPosition, new Color(1, 1, 1));
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
        lastFloorHeight = newHeight;
    }

}
using System;
using Godot;

public partial class CameraFollow : Camera2D
{
    // Export variables for configuration
    [Export] public float LookaheadDistance = 100.0f;
    [Export] public float Damping = 0.1f;
    [Export] public float DeadzoneSize = 50.0f;
    [Export] public float VerticalLockThresholdAbove = 20.0f;
    [Export] public float VerticalLockThresholdBelow = 20.0f;
    private Node2D target;
    private Node2D midTarget;
    private Node2D overrideTarget;
    private bool verticalLocked = true;
    private float floorHeight = 0.0f;

    private Vector2 currentPosition;
    private Vector2 lookAheadOffset;


    public Func<Vector2> TargetVelocityGetter;

    private Vector2 previousFramePosition;

    private int lookAheadDirection = 1;


    public override void _PhysicsProcess(double delta)
    {
        if (target == null)
            return;

        Vector2 targetPosition = target.GlobalPosition;
        Vector2 desiredPosition = currentPosition;

        // handle lookahead based on speed
        var velocity = (targetPosition - previousFramePosition) / (float)delta;
        previousFramePosition = targetPosition;

        if (TargetVelocityGetter != null)
        {
            velocity = TargetVelocityGetter();
        }
        if (velocity.X > 2)
        {
            lookAheadDirection = 1;
        }
        else if (velocity.X < -2)
        {
            lookAheadDirection = -1;
        }
        lookAheadOffset = lookAheadOffset.Lerp(new Vector2(LookaheadDistance * lookAheadDirection, 0), Damping);

        // Deadzone logic
        Rect2 deadzone = new(currentPosition.X - DeadzoneSize / 2, currentPosition.Y - 2000, DeadzoneSize, 4000);
        if (!deadzone.HasPoint(targetPosition))
        {
            // Handle the camera catching up to the target

            if (targetPosition.X > deadzone.End.X)
            {
                desiredPosition.X += targetPosition.X - deadzone.End.X;
            }
            else if (targetPosition.X < deadzone.Position.X)
            {
                desiredPosition.X += targetPosition.X - deadzone.Position.X;
            }
        }

        if (targetPosition.Y < floorHeight - VerticalLockThresholdAbove || targetPosition.Y > floorHeight + VerticalLockThresholdBelow)
        {
            verticalLocked = false;
        }
        GD.Print("Vertical locked: " + verticalLocked);
        // Handle vertical locking
        if (verticalLocked)
        {
            desiredPosition.Y = floorHeight;
        }
        else
        {
            // Damping for vertical movement when not locked
            desiredPosition.Y = targetPosition.Y;
        }

        // Midpoint secondary target
        if (midTarget != null)
        {
            desiredPosition = (desiredPosition + midTarget.GlobalPosition) / 2;
        }

        // Override secondary target
        if (overrideTarget != null)
        {
            desiredPosition = overrideTarget.GlobalPosition;
        }

        // Add lookAhead and smoothly interpolate camera position
        currentPosition = currentPosition.Lerp(desiredPosition, Damping);
        GlobalPosition = currentPosition.Lerp(desiredPosition + lookAheadOffset, Damping);
    }

    // Call this function to set the target
    public void SetTarget(Node2D newTarget)
    {
        target = newTarget;
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
        floorHeight = newHeight;
        verticalLocked = true;
    }
}
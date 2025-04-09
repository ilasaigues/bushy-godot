using BushyCore;
using Godot;
using System;
[Serializable]
public abstract partial class CameraTargetBehaviour : Resource
{
    public CameraFollow Camera { get; set; }

    public void SetSecondaryTarget(Node2D secondaryTarget, bool positionOverride = false)
    {
        if (positionOverride)
        {
            Camera?.SetOverrideTarget(secondaryTarget);
        }
        else
        {
            Camera?.SetMidTarget(secondaryTarget);
        }
    }
    public abstract void SetFloorHeight(float floorHeight);

    public abstract bool ShouldLockVertical { get; }

    public abstract Vector2 GetVelocity(double delta);

    public abstract Node2D TargetNode { get; set; }
}
[Serializable]
public abstract partial class CameraTargetBehaviour<T> : CameraTargetBehaviour where T : Node2D
{
    public override Node2D TargetNode
    {
        get
        {
            return Target;
        }
        set
        {
            if (value is T newTarget)
            {
                Target = newTarget;
            }
        }
    }

    private T _target;

    public T Target
    {
        get
        {
            return _target;
        }
        set
        {
            if (value is T newTarget)
            {
                ChangeTarget(newTarget);
                _target = value;
            }
        }
    }

    protected abstract void ChangeTarget(T newTarget);
}
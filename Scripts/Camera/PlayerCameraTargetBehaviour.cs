using BushyCore;
using System;
using Godot;
using GodotUtilities;

[GlobalClass]
[Serializable]
public partial class PlayerCameraTargetBehaviour : CameraTargetBehaviour<PlayerController>
{

    private Area2D _playerArea;

    public override bool ShouldUpdateLastHeight => Target.MovementComponent.IsOnFloor;

    public PlayerCameraTargetBehaviour()
    {

    }

    public PlayerCameraTargetBehaviour(PlayerController target, CameraFollow camera)
    {
        Target = target;
        Camera = camera;
        _playerArea = target.GetFirstNodeOfType<AreaDetectionComponent>();
        GD.Print(_playerArea);
        _playerArea.AreaEntered += OnAreaEnterPlayer;
        _playerArea.AreaExited += OnAreaExitPlayer;
    }



    public void OnAreaEnterPlayer(Area2D area)
    {
        if (area is CameraSecondaryTarget tgt)
        {
            if (tgt.positionOverride)
            {
                Camera?.SetOverrideTarget(tgt);
            }
            else
            {
                Camera?.SetMidTarget(tgt);
            }
        }
    }

    public void OnAreaExitPlayer(Area2D area)
    {
        if (area is CameraSecondaryTarget)
        {
            Camera?.SetOverrideTarget(null);
            Camera?.SetMidTarget(null);
        }
    }



    public override Vector2 GetFrameVelocity()
    {
        return Target.MovementComponent.RealPositionChange;
    }

    public override void SetFloorHeight(float floorHeight)
    {
        if (Target.MovementComponent.IsOnFloor)
        {
            Camera.UpdateFloorHeight(floorHeight);
        }
    }

    protected override void ChangeTarget(PlayerController newTarget)
    {
        var prevTarget = Target;
        if (_playerArea != null)
        {
            _playerArea.AreaEntered -= OnAreaEnterPlayer;
            _playerArea.AreaExited -= OnAreaExitPlayer;
        }

        _playerArea = newTarget.GetFirstNodeOfType<AreaDetectionComponent>();
        _playerArea.AreaEntered += OnAreaEnterPlayer;
        _playerArea.AreaExited += OnAreaExitPlayer;
    }
}
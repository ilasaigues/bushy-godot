using BushyCore;
using Godot;
using GodotUtilities.Util;
using System;
using System.Diagnostics;

public partial class CameraSecondaryTarget : Area2D
{
    [Export] private bool positionOverride = false;

    public void OnBodyEnter(Node2D body)
    {
        if (body is PlayerController pc)
        {
            pc.SetSecondaryTarget(this, positionOverride);
        }
    }

    public void OnBodyExit(Node2D body)
    {
        if (body is PlayerController pc)
        {
            pc.SetSecondaryTarget(null);
        }
    }
}

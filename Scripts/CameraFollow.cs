using Godot;
using GodotUtilities.Util;
using System;
using System.Diagnostics;
[Tool]
public partial class CameraFollow : Camera2D
{
    [Export]
    CharacterBody2D target;

    bool _editorDirty;

    Vector2 _targetOffset;
    [Export]
    Vector2 TargetOffset
    {
        get => _targetOffset;
        set
        {
            _targetOffset = value;
            _editorDirty = true;
        }
    }


    [ExportGroup("Hard Edges")]
    float _hardRightEdge;
    [Export]
    float HardRightEdge
    {
        get => _hardRightEdge;
        set
        {
            _hardRightEdge = value;
            _editorDirty = true;
        }
    }
    float _hardLeftEdge;
    [Export]
    float HardLeftEdge
    {
        get => _hardLeftEdge;
        set
        {
            _hardLeftEdge = value;
            _editorDirty = true;
        }
    }
    float _hardTopEdge;
    [Export]
    float HardTopEdge
    {
        get => _hardTopEdge;
        set
        {
            _hardTopEdge = value;
            _editorDirty = true;
        }
    }
    float _hardBottomEdge;
    [Export]
    float HardBottomEdge
    {
        get => _hardBottomEdge;
        set
        {
            _hardBottomEdge = value;
            _editorDirty = true;
        }
    }

    [ExportGroup("Soft Edges")]
    float _softRightEdge;
    [Export]
    float SoftRightEdge
    {
        get => _softRightEdge;
        set
        {
            _softRightEdge = value;
            _editorDirty = true;
        }
    }
    float _softLeftEdge;
    [Export]
    float SoftLeftEdge
    {
        get => _softLeftEdge;
        set
        {
            _softLeftEdge = value;
            _editorDirty = true;
        }
    }
    float _softTopEdge;
    [Export]
    float SoftTopEdge
    {
        get => _softTopEdge;
        set
        {
            _softTopEdge = value;
            _editorDirty = true;
        }
    }
    float _softBottomEdge;
    [Export]
    float SoftBottomEdge
    {
        get => _softBottomEdge;
        set
        {
            _softBottomEdge = value;
            _editorDirty = true;
        }
    }

    [ExportGroup("Vertical Snap")]
    float _topSnap;
    [Export]
    float TopSnap
    {
        get => _topSnap;
        set
        {
            _topSnap = value;
            _editorDirty = true;
        }
    }
    float _bottomSnap;
    [Export]
    float BottomSnap
    {
        get => _bottomSnap;
        set
        {
            _bottomSnap = value;
            _editorDirty = true;
        }
    }


    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && _editorDirty)
        {
            _editorDirty = false;
            QueueRedraw();
        }
        if (target == null) return;


        if (Engine.IsEditorHint())
        {
            GlobalPosition = target.GlobalPosition;
            return;
        }

        var targetScreenPos = target.GetGlobalTransformWithCanvas().Origin;


        var screenSize = GetViewportRect().Size;

        float leftLerp = Math.Max(0, Remap(targetScreenPos.X, SoftLeftEdge + HardLeftEdge, HardLeftEdge, 0, 1));
        float rightLerp = Math.Max(0, Remap(targetScreenPos.X, screenSize.X - SoftRightEdge - HardRightEdge, screenSize.X - HardRightEdge, 0, 1));
        float topLerp = Math.Max(0, Remap(targetScreenPos.Y, SoftTopEdge + HardTopEdge, HardTopEdge, 0, 1));
        float bottomLerp = Math.Max(0, Remap(targetScreenPos.Y, screenSize.Y - SoftBottomEdge - HardBottomEdge, screenSize.Y - HardBottomEdge, 0, 1));

        var leftCorrection = GlobalPosition + Vector2.Left * leftLerp / (130 * (float)delta);
        var rightCorrection = GlobalPosition + Vector2.Right * rightLerp / (130 * (float)delta);
        var topCorrection = GlobalPosition + Vector2.Up * topLerp / (130 * (float)delta);
        var bottomCorrection = GlobalPosition + Vector2.Down * bottomLerp / (130 * (float)delta);

        GD.Print(leftLerp);

        var newPos = GlobalPosition
        .LerpUnclamped(leftCorrection, leftLerp) //Math.Pow(leftLerp, 3))
        .LerpUnclamped(rightCorrection, rightLerp) //Math.Pow(rightLerp, 3))
        .LerpUnclamped(topCorrection, topLerp) //Math.Pow(topLerp, 3))
        .LerpUnclamped(bottomCorrection, bottomLerp); //Math.Pow(bottomLerp, 3));

        GlobalPosition = newPos;
    }

    public override void _Draw()
    {
        //if (!Engine.IsEditorHint()) return;

        var screenWidth = Engine.IsEditorHint() ? (float)ProjectSettings.GetSetting("display/window/size/viewport_width") : GetViewportRect().Size.X;
        var screenHeight = Engine.IsEditorHint() ? (float)ProjectSettings.GetSetting("display/window/size/viewport_height") : GetViewportRect().Size.Y;

        var playModeCompensation = Engine.IsEditorHint() ? Vector2.Zero : -GetViewportRect().Position;

        Color hardColor = new(1, 0, 0, 0.2f);
        Color softColor = new(0, 1, 0, 0.2f);
        Color snapLinesColor = new(0, 1, 1);

        Rect2 hardTopRect = new(-screenWidth / 2, -screenHeight / 2, screenWidth, _hardTopEdge);
        hardTopRect.Position += playModeCompensation;
        DrawRect(hardTopRect, hardColor);

        Rect2 hardBottomRect = new(-screenWidth / 2, screenHeight / 2 - _hardBottomEdge, screenWidth, _hardBottomEdge);
        hardBottomRect.Position += playModeCompensation;
        DrawRect(hardBottomRect, hardColor);

        Rect2 hardLeftRect = new(-screenWidth / 2, -screenHeight / 2, _hardLeftEdge, screenHeight);
        hardLeftRect.Position += playModeCompensation;
        DrawRect(hardLeftRect, hardColor);

        Rect2 hardRightRect = new(screenWidth / 2 - _hardRightEdge, -screenHeight / 2, _hardRightEdge, screenHeight);
        hardRightRect.Position += playModeCompensation;
        DrawRect(hardRightRect, hardColor);

        Rect2 softTopRect = new(-screenWidth / 2 + _hardLeftEdge, -screenHeight / 2 + _hardTopEdge, screenWidth - _hardLeftEdge - _hardRightEdge, _softTopEdge);
        softTopRect.Position += playModeCompensation;
        DrawRect(softTopRect, softColor);

        Rect2 softBottomRect = new(-screenWidth / 2 + _hardLeftEdge, screenHeight / 2 - _hardBottomEdge - _softBottomEdge, screenWidth - _hardLeftEdge - _hardRightEdge, _softBottomEdge);
        softBottomRect.Position += playModeCompensation;
        DrawRect(softBottomRect, softColor);

        Rect2 softLeftRect = new(-screenWidth / 2 + _hardLeftEdge, -screenHeight / 2 + _hardTopEdge, _softLeftEdge, screenHeight - _hardTopEdge - _hardBottomEdge);
        softLeftRect.Position += playModeCompensation;
        DrawRect(softLeftRect, softColor);

        Rect2 softRightRect = new(screenWidth / 2 - _hardRightEdge - _softRightEdge, -screenHeight / 2 + _hardTopEdge, _softRightEdge, screenHeight - _hardTopEdge - _hardBottomEdge);
        softRightRect.Position += playModeCompensation;
        DrawRect(softRightRect, softColor);

        DrawLine(new Vector2(-screenWidth / 2, -screenHeight / 2 + _topSnap), new Vector2(screenWidth / 2, -screenHeight / 2 + _topSnap), snapLinesColor);
        DrawLine(new Vector2(-screenWidth / 2, screenHeight / 2 - _bottomSnap), new Vector2(screenWidth / 2, screenHeight / 2 - _bottomSnap), snapLinesColor);

    }

    public static float Remap(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        return toLow + (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow);
    }
}

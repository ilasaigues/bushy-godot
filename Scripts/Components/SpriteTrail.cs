using Godot;
using System;

public partial class SpriteTrail : Sprite2D
{
    [Export] private Sprite2D _baseTrail;

    private Vector2 _previousPosition;

    public override void _Ready()
    {
        _previousPosition = _baseTrail.GlobalPosition;
        Frame = _baseTrail.Frame;
    }

    public override void _PhysicsProcess(double delta)
    {
        FlipH = _baseTrail.FlipH;
        Frame = _baseTrail.Frame;
        var offset = (_previousPosition - _baseTrail.GlobalPosition) * 2;
        GlobalPosition = _baseTrail.GlobalPosition + offset;

        _previousPosition = _baseTrail.GlobalPosition;
    }
}

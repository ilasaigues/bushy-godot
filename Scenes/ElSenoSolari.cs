using Godot;
using System;

public partial class ElSenoSolari : Sprite2D
{
    float time;
    public override void _PhysicsProcess(double delta)
    {
        time += (float)delta;
        GlobalPosition += Vector2.Up * MathF.Sin(time * 2) * 0.2f;

        GD.Print(MathF.Sin((float)delta));
    }


}

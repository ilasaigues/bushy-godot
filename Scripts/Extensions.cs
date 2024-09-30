using Godot;
using System;

public static class Extensions
{
    public static Vector2 LerpUnclamped(this Vector2 a, Vector2 b, float t)
    {
        return a * (1 - t) + b * t;
    }
    public static Vector2 LerpUnclamped(this Vector2 a, Vector2 b, double t)
    {
        return a.LerpUnclamped(b, (float)t);
    }

    public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t)
    {
        return a * (1 - t) + b * t;
    }

    public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, double t)
    {
        return a.LerpUnclamped(b, (float)t);
    }
}

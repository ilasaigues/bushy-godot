using System;
using Godot;

public static class Utils
{
    public static T PrintInPlace<T>(this T obj, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            message = string.Format(message, obj.ToString());
        }
        else
        {
            message = obj.ToString();
        }
        GD.Print(message);
        return obj;
    }

    public static T WarnInPlace<T>(this T obj, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
        {
            message = string.Format(message, obj.ToString());
        }
        else
        {
            message = obj.ToString();
        }
        GD.PushWarning(message);
        return obj;
    }
}
public static class Vector2Extension
{

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        var deg2Rad = Math.PI / 180;
        float sin = (float)Mathf.Sin(degrees * deg2Rad);
        float cos = (float)Mathf.Cos(degrees * deg2Rad);

        float tx = v.X;
        float ty = v.Y;
        v.X = (cos * tx) - (sin * ty);
        v.Y = (sin * tx) + (cos * ty);
        return v;
    }
}
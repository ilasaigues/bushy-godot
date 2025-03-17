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
}
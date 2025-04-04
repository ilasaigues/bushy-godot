using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class AnimationController : Node
{
    [Export] public AnimationTree AnimationTree { get; set; }
    private HashSet<string> _triggers = [];

    private Dictionary<string, string> NameToPath = [];

    private const bool DEBUG = false;

    public void InitializeFromType(Type nameContainerType)
    {
        if (DEBUG) nameContainerType.Name.WarnInPlace("DEBUG: Initializing from type {0}");
        foreach (var field in nameContainerType.GetFields())
        {
            NameToPath[field.Name] = (string)field.GetRawConstantValue();
        }
    }

    public void SetCondition(string conditionPath, bool value)
    {
        ValidateString(conditionPath);
        if (NameToPath.TryGetValue(conditionPath, out var actualPath))
        {
            conditionPath = actualPath;
        }
        if (DEBUG) conditionPath.WarnInPlace($"DEBUG: Setting condition {{0}} to {value}");

        AnimationTree.Set(conditionPath, value);
    }

    public bool GetCondition(string conditionPath)
    {
        ValidateString(conditionPath);
        if (NameToPath.TryGetValue(conditionPath, out var actualPath))
        {
            conditionPath = actualPath;
        }
        return AnimationTree.Get(conditionPath).AsBool();
    }

    public void SetBlendValue(string blendPath, Vector2 blendValue)
    {
        ValidateString(blendPath);
        if (NameToPath.TryGetValue(blendPath, out var actualPath))
        {
            blendPath = actualPath;
        }
        AnimationTree.Set(blendPath, blendValue);
    }


    public void SetTrigger(string triggerName)
    {
        ValidateString(triggerName);
        if (NameToPath.TryGetValue(triggerName, out var actualPath))
        {
            triggerName = actualPath;
        }
        if (DEBUG) triggerName.WarnInPlace($"DEBUG: Setting trigger {{0}}");

        _triggers.Add(triggerName);
    }

    public void UnsetTrigger(string triggerName)
    {
        ValidateString(triggerName);
        if (NameToPath.TryGetValue(triggerName, out var actualPath))
        {
            triggerName = actualPath;
        }
        _triggers.Remove(triggerName);
    }

    public bool CheckTrigger(string triggerName)
    {
        ValidateString(triggerName);
        if (NameToPath.TryGetValue(triggerName, out var actualPath))
        {
            triggerName = actualPath;
        }
        if (_triggers.Contains(triggerName))
        {
            _triggers.Remove(triggerName);
            return true;
        }
        return false;
    }

    private static void ValidateString(string str, [CallerMemberName] string methodName = "")
    {
        if (string.IsNullOrEmpty(str))
        {
            GD.PushWarning($"Function {methodName} invoked with empty string.");
        }
    }
}

using BushyCore;
using Godot;
using System;

[Tool]
[GlobalClass]
public partial class BoolEvent : BaseEventResource<bool>
{
    [Export]
    protected override bool _editorValue { get; set; }
    [Export]
    private bool CallWithEditorValue
    {
        get { return false; }
        set
        {
            TriggerEvent(_editorValue);
        }
    }
}

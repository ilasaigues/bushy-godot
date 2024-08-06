using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node
{
    public static InputManager Instance { get; private set; }
    public InputAction JumpAction { get; private set; } = new("ui_accept");
    public InputAction LeftAction { get; private set; } = new("ui_left");

    private List<InputAction> _inputActions = new();

    public override void _Ready()
    {
        Instance ??= this;
        _inputActions = new List<InputAction> {
            JumpAction,
            LeftAction,
         };
    }

    public override void _Input(InputEvent @event)
    {
        _inputActions.ForEach(inputAction => inputAction.PassInputEvent(@event));
    }

    public override void _Process(double delta)
    {
        _inputActions.ForEach(inputAction => inputAction.Process(delta));
    }
}
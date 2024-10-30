using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node
{
    public static InputManager Instance { get; private set; }
    public InputAction JumpAction { get; private set; } = new("ui_accept");
    public InputAction LeftAction { get; private set; } = new("ui_left");
    public InputAction RightAction { get; private set; } = new("ui_right");
    public InputAction UpAction { get; private set; } = new("ui_up");
    public InputAction DownAction { get; private set; } = new("ui_down");
    public InputAction DashAction { get; private set; } = new("left_shift");
    public InputAction AttackAction { get; private set; } = new("game_attack");
    public InputAxis HorizontalAxis { get; private set; }
    public InputAxis VerticalAxis { get; private set; }

    private List<InputAction> _inputActions = new();

    public override void _Ready()
    {
        Instance ??= this;

        HorizontalAxis = new InputAxis(RightAction, LeftAction);
        VerticalAxis = new InputAxis(DownAction, UpAction);

        _inputActions = new List<InputAction> {
            DashAction,
            JumpAction,
            LeftAction,
            RightAction,
            UpAction,
            DownAction,
            AttackAction
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
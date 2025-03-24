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
    public InputAction BurstAction { get; private set; } = new("game_burst");
    public InputAction HarpoonAction { get; private set; } = new("game_harpoon");
    public InputAxis HorizontalAxis { get; private set; }
    public InputAxis VerticalAxis { get; private set; }

    public List<InputAction> InputActions { get; private set; } = new();

    public override void _Ready()
    {
        Instance ??= this;

        HorizontalAxis = new InputAxis(RightAction, LeftAction);
        VerticalAxis = new InputAxis(DownAction, UpAction);

        InputActions = new List<InputAction> {
            DashAction,
            JumpAction,
            LeftAction,
            RightAction,
            UpAction,
            DownAction,
            AttackAction,
            BurstAction,
            HarpoonAction
         };
    }

    public override void _Input(InputEvent @event)
    {
        InputActions.ForEach(inputAction => inputAction.PassInputEvent(@event));
    }

    public override void _Process(double delta)
    {
        InputActions.ForEach(inputAction => inputAction.Process(delta));
    }
}
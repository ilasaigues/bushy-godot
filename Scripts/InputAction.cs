using Godot;
using System;

public class InputAction
{
    public float TimePressed => Pressed ? (Time.GetTicksMsec() - _timeLastPressed) / 1000f : float.MinValue;
    private float _timeLastPressed = 0;
    public string ActionID { get; private set; }
    public bool Pressed { get; private set; }

    public Action OnInputJustPressed = () => { };
    public Action WhileInputPressed = () => { };
    public Action OnInputReleased = () => { };

    public InputAction(string actionID)
    {
        this.ActionID = actionID;
    }

    public void PassInputEvent(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(ActionID))
        {
            if (!Pressed)
            {
                Pressed = true;
                _timeLastPressed = Time.GetTicksMsec();
                OnInputJustPressed();
            }
            WhileInputPressed();
        }
        if (inputEvent.IsActionReleased(ActionID))
        {
            Pressed = false;
            _timeLastPressed = -1;
            OnInputReleased();
        }
    }

    public void Process(double delta)
    {
        if (Pressed)
        {
            WhileInputPressed();
        }
    }
}

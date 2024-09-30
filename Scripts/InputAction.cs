using Godot;
using System;
using System.Diagnostics;

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
        }
        if (inputEvent.IsActionReleased(ActionID))
        {
            if (ActionID == "left_shift") Debug.WriteLine("Dash INPUT CANCELLED");
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

public class InputAxis
{
    private InputAction _positiveAction;
    private InputAction _negativeAction;

    public float Value => (_positiveAction.Pressed ? 1 : 0) - (_negativeAction.Pressed ? 1 : 0);

    public Action<float> OnAxisUpdated = _ => { };

    public InputAxis(InputAction positive, InputAction negative)
    {
        _positiveAction = positive;
        _negativeAction = negative;
        _positiveAction.OnInputJustPressed += OnActionsUpdated;
        _positiveAction.OnInputReleased += OnActionsUpdated;
        _negativeAction.OnInputJustPressed += OnActionsUpdated;
        _negativeAction.OnInputReleased += OnActionsUpdated;
    }

    private void OnActionsUpdated()
    {
        OnAxisUpdated(Value);
    }

}
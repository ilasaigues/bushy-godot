using Godot;
using System;
using System.Diagnostics;

public class DisposableBinding : IDisposable
{
    private Action OnDispose;
    public DisposableBinding(Action onDispose)
    {
        OnDispose = onDispose;
    }
    public void Dispose()
    {
        OnDispose();
    }
}

public class InputAction
{
    public enum InputActionType
    {
        InputPressed,
        InputReleased,
        InputHeld,
    }

    public float TimeHeld => Pressed ? (Time.GetTicksMsec() - _timeLastPressed) / 1000f : float.MinValue;
    public float TimeSinceLastPressed => (Time.GetTicksMsec() - _timeLastPressed) / 1000f;
    private float _timeLastPressed = 0;
    public string ActionID { get; private set; }
    public bool Pressed { get; private set; }

    private Action<InputAction> OnInputJustPressed = _ => { };
    private Action<InputAction> WhileInputPressed = _ => { };
    private Action<InputAction> OnInputReleased = _ => { };

    public InputAction(string actionID)
    {
        ActionID = actionID;
    }

    public DisposableBinding BindToInputJustPressed(Action<InputAction> handler)
    {
        OnInputJustPressed += handler;
        return new DisposableBinding(() => OnInputJustPressed -= handler);
    }

    public DisposableBinding BindToInputHeld(Action<InputAction> handler)
    {
        WhileInputPressed += handler;
        return new DisposableBinding(() => WhileInputPressed -= handler);
    }

    public DisposableBinding BindToInputReleased(Action<InputAction> handler)
    {
        OnInputReleased += handler;
        return new DisposableBinding(() => OnInputReleased -= handler);
    }


    public void PassInputEvent(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed(ActionID))
        {
            if (!Pressed)
            {
                Pressed = true;
                _timeLastPressed = Time.GetTicksMsec();
                OnInputJustPressed(this);
            }
        }
        if (inputEvent.IsActionReleased(ActionID))
        {
            Pressed = false;
            _timeLastPressed = -1;
            OnInputReleased(this);
        }
    }

    public void Process(double delta)
    {
        if (Pressed)
        {
            WhileInputPressed(this);
        }
    }
}

public class InputAxis
{
    private InputAction _positiveAction;
    private InputAction _negativeAction;

    public float Value => (_positiveAction.Pressed ? 1 : 0) - (_negativeAction.Pressed ? 1 : 0);

    private Action<InputAxis> OnAxisUpdated = _ => { };

    public DisposableBinding BindToAxisUpdated(Action<InputAxis> handler)
    {
        OnAxisUpdated += handler;
        return new DisposableBinding(() => OnAxisUpdated -= handler);
    }

    public InputAxis(InputAction positive, InputAction negative)
    {
        _positiveAction = positive;
        _negativeAction = negative;
        _positiveAction.BindToInputJustPressed(OnActionsUpdated);
        _positiveAction.BindToInputReleased(OnActionsUpdated);
        _negativeAction.BindToInputJustPressed(OnActionsUpdated);
        _negativeAction.BindToInputReleased(OnActionsUpdated);
    }

    private void OnActionsUpdated(InputAction action)
    {
        OnAxisUpdated(this);
    }

}

// TODO: Separate binary axes (keyboard) from analogue axes to support controllers
using System;
using Godot;

public abstract partial class BaseState : Node {
    protected MovementComponent movementComponent;
    public void InitState(MovementComponent mc) {
        this.movementComponent = mc;
    }

    public abstract void UpdateInternal(double delta);
}
using BushyCore;
using Godot;
using System;

[GlobalClass]
public partial class PhaseCoreography : Resource
{
    [Export]
    public AttackStep.AttackStepPhase Phase { get; set; }
    [Export]
    public float TimerDuration { get; set; }
    [Export]
    public Vector2 VelocityVector { get; set; }
    [Export]
    public Vector2 AccelerationVector { get; set; }
    [Export]
    public bool BeginOnTimerEnd { get; set; }
}

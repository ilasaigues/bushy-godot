using Godot;
using System;

public partial class AnimationComponent : AnimationPlayer
{
	// Emits a signal to indicate a phase/step change within an animaiton track
	[Signal]
	public delegate void AnimationStepChangeEventHandler(int phase);
	public void ChangeAnimationStep(int phase) => EmitSignal(SignalName.AnimationStepChange, phase);
}

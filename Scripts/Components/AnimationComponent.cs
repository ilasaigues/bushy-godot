using Godot;
using System;

public partial class AnimationComponent : AnimationPlayer
{
	// Emits a signal to indicate a phase/step change within an animaiton track
	[Signal]
	public delegate void AnimationStepChangeEventHandler(int step);
	public void ChangeAnimationStep(int step)
	{
		EmitSignal(SignalName.AnimationStepChange, step);
	}
}

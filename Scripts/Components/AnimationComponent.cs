using Godot;
using System;

public partial class AnimationComponent : AnimationPlayer
{
	[Signal]
	public delegate void AnimationStepChangeEventHandler(int step);
	public void ChangeAnimationStep(int step)
	{
		EmitSignal(SignalName.AnimationStepChange, step);
	}
}

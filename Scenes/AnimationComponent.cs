using Godot;
using System;

public partial class AnimationComponent : AnimationPlayer
{
	AnimationLibrary library;

	void PlayAnimation(Animation newAnimation, bool overrideCurrentAnimation)
	{
		if(library == null) library = new AnimationLibrary();
		Animation current_anim = IsPlaying() ? GetAnimation(CurrentAnimation) : null;
		if(!library.HasAnimation(newAnimation.ResourceName))
		{
			library.AddAnimation(newAnimation.ResourceName, newAnimation);
		}
		if(current_anim != null && current_anim.LoopMode != Animation.LoopModeEnum.Linear && !overrideCurrentAnimation)
		{
			ClearQueue();
			AnimationSetNext(CurrentAnimation, "animations/" + newAnimation.ResourceName);
		}
		else
		{
			Stop();
			Play("animations/" + newAnimation.ResourceName);
		}
	}
}

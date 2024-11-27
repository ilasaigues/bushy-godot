using Godot;
using System;

public partial class VisualComponent : Node2D
{
	private Sprite2D sprite;
	private AnimationPlayer animPlayer;
	AnimationLibrary library;

    public override void _Ready()
    {
       	
    }

    public void Init()
	{
		library = new AnimationLibrary();
		for (var i = 0; i < GetChildCount(); i++)
		{
			if(GetChild(i).IsClass("Sprite2D")) sprite = GetChild(i) as Sprite2D;
			if(GetChild(i).IsClass("AnimationPlayer")) animPlayer = GetChild(i) as AnimationPlayer;
		}
		animPlayer.AddAnimationLibrary("animations", library);
	}

	public void PlayAnimation(Animation newAnimation, bool overrideCurrentAnimation)
	{
		if(library == null) 
		{
			library = new AnimationLibrary();
			animPlayer.AddAnimationLibrary("animations", library);
		}
		Animation current_anim = animPlayer.IsPlaying() ? animPlayer.GetAnimation(animPlayer.CurrentAnimation) : null;
		if(!library.HasAnimation(newAnimation.ResourceName))
		{
			library.AddAnimation(newAnimation.ResourceName, newAnimation);
		}
		if(current_anim != null && current_anim.LoopMode != Animation.LoopModeEnum.Linear && !overrideCurrentAnimation)
		{
			animPlayer.ClearQueue();
			animPlayer.Queue("animations/" + newAnimation.ResourceName);
		}
		else
		{
			animPlayer.Stop();
			animPlayer.Play("animations/" + newAnimation.ResourceName);
		}
	}
}

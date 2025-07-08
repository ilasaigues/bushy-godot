using Godot;
using System;

public partial class SubAnimationController : Sprite2D
{
    [Export] AnimationPlayer _subPlayer;
    public void SetSubAnimation(string subAnimationName)
    {
        _subPlayer.Play(subAnimationName);
    }
}

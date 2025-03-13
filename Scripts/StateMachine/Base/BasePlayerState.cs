using System;
using Godot;

namespace BushyCore
{
    public abstract partial class BasePlayerState : BaseState<PlayerController>
    {
        [Export] public override PlayerController Agent { get; set; }
    }
}
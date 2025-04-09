using Godot;

namespace BushyCore
{
    [GlobalClass]
    public partial class ComboData : Resource
    {
        [Export] public ComboAttackData[] Attacks;
        [Export] public bool GravityEnabled = true;
        [Export] public bool Loop = false;
    }
}
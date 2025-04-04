using Godot;

namespace BushyCore
{
    [GlobalClass]
    public partial class ComboAttackData : Resource
    {
        [Export] public float AttackTimer;
        [Export] public float NextComboTimer;
        [Export] public int Damage;
        [Export] public Vector2 NudgeDirection;
        [Export] public float HitStopTime;
    }
}
using Godot;

namespace BushyCore
{
    partial class BasicAttackStep : AttackStep
    {
        public override void StepEnter() {
            EmitSignal(SignalName.BattleAnimationChange, "basic_attack");
        }   
    }
}
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    abstract partial class AttackStep : Node2D
    {
        [Signal]
        public delegate void BattleAnimationChangeEventHandler(string animationKey);

        public void InitState() {}   
        public virtual void CombatUpdate(double delta) {}
        public virtual void StepEnter() {}

    }
}
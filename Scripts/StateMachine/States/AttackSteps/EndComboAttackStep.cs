namespace BushyCore
{
    partial class EndComboAttackStep : AttackStep
    {

        public override void StepEnter() {
            base.StepEnter();
            EmitSignal(SignalName.BattleAnimationChange, "ground_attack_3");
        }

    }
}
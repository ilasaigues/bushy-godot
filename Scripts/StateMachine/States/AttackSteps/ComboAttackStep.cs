using Godot;

namespace BushyCore
{
    partial class ComboAttackStep : AttackStep
    {
        [Export]
        public AttackStep BasicAttackCombo_3;
        private bool bufferComboAttack = false;
        public override void StepEnter() {
            base.StepEnter();

            bufferComboAttack = false;
            EmitSignal(SignalName.BattleAnimationChange, "ground_attack_2");
        }

        public override void CombatUpdate(double delta)
        {
            if (currentPhase == AttackStepPhase.RECOVERY && bufferComboAttack)
                EmitSignal(SignalName.ComboStep, BasicAttackCombo_3);

            base.CombatUpdate(delta);
        }

        public override void HandleAttackAction()
        {
            switch (currentPhase) {
                case AttackStepPhase.ACTION:
                    bufferComboAttack = true;
                    break;
                case AttackStepPhase.RECOVERY:
                    EmitSignal(SignalName.ComboStep, BasicAttackCombo_3);
                    break;
                default: 
                    break;
            }
        }
    }
}
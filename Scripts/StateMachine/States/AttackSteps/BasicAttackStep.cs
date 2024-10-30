using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;

namespace BushyCore
{
    partial class BasicAttackStep : AttackStep
    {
        [Export]
        public AttackStep BasicAttackCombo_2;
        private bool bufferComboAttack = false;
        public override void StepEnter() {
            base.StepEnter();

            bufferComboAttack = false;
            EmitSignal(SignalName.BattleAnimationChange, "ground_attack_1");
        }

        public override void CombatUpdate(double delta)
        {
            if (currentPhase == AttackStepPhase.RECOVERY && bufferComboAttack)
                EmitSignal(SignalName.ComboStep, BasicAttackCombo_2);

            base.CombatUpdate(delta);
        }

        public override void HandleAttackAction()
        {
            switch (currentPhase) {
                case AttackStepPhase.ACTION:
                    bufferComboAttack = true;
                    break;
                case AttackStepPhase.RECOVERY:
                    EmitSignal(SignalName.ComboStep, BasicAttackCombo_2);
                    break;
                default: 
                    break;
            }
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    partial class BasicAttackStep : AttackStep
    {
        // Estos exports esta bien que se pasen por editor?
        [Export]
        public AttackStep BasicAttackCombo_2;
        private bool bufferComboAttack = false;

        public override void StepEnter(AttackStepConfig config) {
            this.AddToGroup();
            this.WireNodes();

            bufferComboAttack = false;
            base.StepEnter(config);
        }

        public override void CombatUpdate(double delta)
        {
            if ((currentPhase == AttackStepPhase.RECOVERY 
                || currentPhase == AttackStepPhase.COMBO)
                && bufferComboAttack)
                EmitSignal(SignalName.ComboStep, BasicAttackCombo_2, attackStepConfigs);

            base.CombatUpdate(delta);
        }

        public override void HandleAttackAction()
        {
            switch (currentPhase) {
                case AttackStepPhase.ACTION:
                    bufferComboAttack = true;
                    break;
                case AttackStepPhase.COMBO:
                case AttackStepPhase.RECOVERY:
                    EmitSignal(SignalName.ComboStep, BasicAttackCombo_2, attackStepConfigs);
                    break;
                default: 
                    break;
            }
        }
    }
}
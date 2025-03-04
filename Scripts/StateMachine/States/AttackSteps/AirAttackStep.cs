using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    partial class AirAttackStep : AttackStep
    {
        // Estos exports esta bien que se pasen por editor?
        [Export]
        public AttackStep AirAttackCombo_2;
        private bool bufferComboAttack = false;

        public override void StepEnter(AttackStepConfig config) {
            this.AddToGroup();
            this.WireNodes();

            bufferComboAttack = false;
            base.StepEnter(config);
        }

        public override void CombatUpdate(double delta)
        {
            if (currentPhase == AttackStepPhase.RECOVERY && bufferComboAttack)
                EmitComboIfAirborne();
                
            base.CombatUpdate(delta);
        }

        public override void HandleAttackAction()
        {
            switch (currentPhase) {
                case AttackStepPhase.ACTION:
                case AttackStepPhase.COMBO:
                    bufferComboAttack = true;
                    break;
                case AttackStepPhase.RECOVERY:
                    EmitComboIfAirborne();
                    break;
                default: 
                    break;
            }
        }

        private void EmitComboIfAirborne()
        {
            if (!this.MovementComponent.IsOnFloor)
                EmitSignal(SignalName.ComboStep, AirAttackCombo_2, attackStepConfigs);
        }
    }
}
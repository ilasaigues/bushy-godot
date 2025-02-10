using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    partial class AirComboAttackStep : AttackStep
    {
        [Export]
        public AttackStep AirAttackCombo_1;
        private bool bufferComboAttack = false;

        public override void StepEnter(AttackStepConfig config) {
            this.AddToGroup();
            this.WireNodes();

            bufferComboAttack = false;
            base.StepEnter(config);
        }

        public override void CombatUpdate(double delta)
        {
            var airBorne = !this.MovementComponent.IsOnFloor;
            if (airBorne && currentPhase == AttackStepPhase.RECOVERY && bufferComboAttack)
                EmitSignal(SignalName.ComboStep, AirAttackCombo_1, attackStepConfigs);

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
                    EmitSignal(SignalName.ComboStep, AirAttackCombo_1, attackStepConfigs);
                    break;
                default: 
                    break;
            }
        }
    }
}
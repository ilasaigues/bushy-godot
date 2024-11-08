using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    partial class BasicAttackStep : AttackStep
    {
        [Node]
        public Timer AttackMovementTimer;
        // Estos exports esta bien que se pasen por editor?
        [Export]
        public AttackStep BasicAttackCombo_2;
        private bool bufferComboAttack = false;

        // Hace falta emitir un evento de animation change?
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

        protected override void CoreographMovement()
        {
            switch(currentPhase) {
                case AttackStepPhase.ACTION:
                    AttackMovementTimer.Start();
                    EmitSignal(SignalName.ForceCoreography, attackMovement * attackStepConfigs.Direction.Normalized());
                    break;
                default:
                    break;
            }
        }

        void OnAttackMovementTimerEnd()
        {
            EmitSignal(SignalName.ForceCoreography, Vector2.Zero);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

    }
}
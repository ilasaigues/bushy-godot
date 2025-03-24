using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    partial class ComboAttackStep : AttackStep
    {
        [Export]
        public AttackStep BasicAttackCombo_3;
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
                EmitSignal(SignalName.ComboStep, BasicAttackCombo_3, attackStepConfigs);

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
                    EmitSignal(SignalName.ComboStep, BasicAttackCombo_3, attackStepConfigs);
                    break;
                default: 
                    break;
            }
        }

        private Shape2D _DebugHitboxShape;
        [Export]
        protected Shape2D DebugHitboxShape { 
            get { return _DebugHitboxShape; }
            set {
                if (!DebugHitbox) return;
                if (_DebugHitboxShape != null) {
                    _DebugHitboxShape.Changed -= QueueRedraw;
                    _DebugHitboxShape.Changed -= RemoveToolRef;
                }
                
                hitboxShape = value;
                _DebugHitboxShape = value;

                if (_DebugHitboxShape != null) {
                    _DebugHitboxShape.Changed += QueueRedraw;
                    _DebugHitboxShape.Changed += RemoveToolRef;
                }

                QueueRedraw();
        }}

        public void RemoveToolRef() 
        {
            hitboxShape = _DebugHitboxShape;
        }
    }
}
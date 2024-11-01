using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    [Tool]
    partial class BasicAttackStep : AttackStep
    {
       
        // Estos exports esta bien que se pasen por editor?
        [Export]
        public AttackStep BasicAttackCombo_2;
        private bool bufferComboAttack = false;

        // Hace falta emitir un evento de animation change?
        public override void StepEnter(AttackStepConfig config) {
            bufferComboAttack = false;
            base.StepEnter(config);
        }

        public override void _Ready()
        {
            hitboxShape = DebugHitboxShape;
            base._Ready();
        }
        public override void CombatUpdate(double delta)
        {
            if (currentPhase == AttackStepPhase.RECOVERY 
                && currentPhase == AttackStepPhase.COMBO
                && bufferComboAttack)
                EmitSignal(SignalName.ComboStep, BasicAttackCombo_2);

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


         // Explicacion. Trate de hacer una herramienta de heditor que nos permita visualizar la hitbox de este script.
        // El tema es que esa hitbox es una property HEREDADA y EXPORTABLE de un parent tool script.
        // Por default si nosotros dejamos este setter en el parent tool script, funciona de diez, hasta que cambias de escena
        // En ese momento, el valor exportado se pierde y listo. Esto es un hack malisimo para poder visualizar el hitbox durante la edicion
        private Shape2D _DebugHitboxShape;
        [Export]
        protected Shape2D DebugHitboxShape { 
            get { return _DebugHitboxShape; }
            set {
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
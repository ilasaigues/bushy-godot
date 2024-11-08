using GodotUtilities;
using Godot;
using System.Diagnostics;

namespace BushyCore
{
    [Scene]
    partial class EndComboAttackStep : AttackStep
    {
        [Node]
        public Timer AttackMovementTimer;

        public override void StepEnter(AttackStepConfig config) {
            this.AddToGroup();
            this.WireNodes();
            
            base.StepEnter(config);
        }
        protected override void CoreographMovement()
        {
            switch(currentPhase) {
                case AttackStepPhase.WINDUP:
                    AttackMovementTimer.Start();
                    Debug.WriteLine(attackStepConfigs.Direction);
                    Debug.WriteLine(attackMovement);
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
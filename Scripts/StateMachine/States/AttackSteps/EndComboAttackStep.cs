using GodotUtilities;
using Godot;

namespace BushyCore
{
    [Scene]
    [Tool]
    partial class EndComboAttackStep : AttackStep
    {
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
using Godot;

namespace BushyCore
{
    public partial class AttackStepConfig : GodotObject
    {
        public Vector2 Direction;
        public AttackStepConfig(Vector2 direction) { Direction = direction; }
    }
}
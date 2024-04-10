using Godot;

namespace BushyCore 
{
    [Tool]
    public partial class CharacterVariables : Resource
    {
        [Export] 
        public int GroundHorizontalAcceleration { get; private set; }
        [Export] 
        public int GroundHorizontalDeceleration { get; private set; }
        [Export] 
        public int GroundHorizontalMovementSpeed { get; private set; }

        public CharacterVariables() {}
    }
}
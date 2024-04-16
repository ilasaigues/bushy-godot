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

        [Export]
        public float JumpSpeed { get; private set; }
        [Export]
        public float JumpDuration { get; private set; }
        [Export]
        public float AirHorizontalDeceleration { get; private set; }
        [Export]
        public float AirHorizontalMovementSpeed { get; private set; }
        [Export]
        public float AirHorizontalAcceleration { get; private set; }
        [Export]
        public float AirHorizontalOvercappedDeceleration { get; private set; }
        [Export]
        public float JumpShortHopSpeed { get; private set; }
        [Export]
        public float AirTerminalVelocity { get; private set; }

        [Export]
        public float AirGravity { get; private set; }
        [Export]
        public Vector2 AirSpeedThresholds { get; private set; }
        [Export]
        public float AirApexGravity { get; private set; }
        

        public CharacterVariables() {}
    }
}
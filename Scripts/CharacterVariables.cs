using Godot;

namespace BushyCore
{
    [Tool]
    public partial class CharacterVariables : Resource
    {
        [ExportCategory("Grounded")]
        [Export]
        public int GroundHorizontalAcceleration { get; private set; }
        [Export]
        public int GroundHorizontalDeceleration { get; private set; }
        [Export]
        public int HorizontalTurnDeceleration { get; private set; }
        [Export]
        public int GroundHorizontalMovementSpeed { get; private set; }
        [Export]
        public int GroundHorizontalOvercappedDeceleration { get; private set; }
        [Export]
        public int MaxOnWallHorizontalMovementSpeed { get; private set; }
        [ExportCategory("Air")]
        [Export]
        public int JumpSpeed { get; private set; }
        [Export]
        public float JumpDuration { get; private set; }
        [Export]
        public float JumpCoyoteTime { get; private set; }
        [Export]
        public float JumpBufferTime { get; private set; }
        [Export]
        public int AirHorizontalDeceleration { get; private set; }
        [Export]
        public int AirHorizontalMovementSpeed { get; private set; }
        [Export]
        public int AirHorizontalAcceleration { get; private set; }
        [Export]
        public int AirHorizontalOvercappedDeceleration { get; private set; }
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
        [ExportCategory("Dash")]
        [Export]
        public float DashInitTime { get; private set; }
        [Export]
        public float DashTime { get; private set; }
        [Export]
        public float DashExitTime { get; private set; }
        [Export]
        public float DashVelocity { get; private set; }
        [Export]
        public float DashExitVelocity { get; private set; }
        [Export]
        public float DashCooldown { get; private set; }
        [Export]
        public float DashJumpWindow { get; private set; }
        [Export]
        public float DashJumpSpeed { get; private set; }

        [ExportCategory("Hedge")]
        [Export]
        public float HedgeEnteringWaitTime { get; private set; }
        [Export]
        public float HedgeDashBufferTime { get; set; }
        [Export]
        public float HedgeJumpBufferTime { get; set; }
        [Export]
        public int HedgeAcceleration { get; private set; }
        [Export]
        public int HedgeDeceleration { get; private set; }
        [Export]
        public int HedgeTurnDeceleration { get; private set; }
        [Export]
        public int HedgeOvercappedDeceleration { get; private set; }
        [Export]
        public int HedgeMovementSpeed { get; private set; }
        [Export]
        public int MaxHedgeEnterSpeed { get; private set; }
        public CharacterVariables() { }
    }
}
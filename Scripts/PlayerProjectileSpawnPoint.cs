using Godot;

namespace BushyCore
{
    public partial class PlayerProjectileSpawnPoint : Node2D
    {
        [Export] Vector2Event _positionChangedEvent;

        public void ReportPosition()
        {
            _positionChangedEvent.TriggerEvent(GlobalPosition);
        }
    }
}
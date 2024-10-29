using System.Diagnostics;
using Godot;
using GodotUtilities;
using static MovementComponent;

namespace BushyCore 
{
    [Scene]
    public partial class BattleState : BaseState
    {
        [Node]
        CombatStateMachine CombatStateMachine;
        protected override void AnimationUpdate()
        {
            throw new System.NotImplementedException();
        }

        protected override void VelocityUpdate()
        {
            throw new System.NotImplementedException();
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
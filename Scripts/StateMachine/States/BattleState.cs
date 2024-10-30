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
        private int comboCounter;

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnterInternal(configs);
            CombatStateMachine.ChangeAttackStep<BasicAttackStep>();
        }
        public override void StateUpdateInternal(double delta)
        {
            CombatStateMachine.CombatUpdate(delta);
            // Movement component changes
        } 

        public void OnBattleAnimationChange(string animationKey)
        {
            animationComponent.Play(animationKey);
        }

        public void OnBattleEnd()
        {
            RunAndEndState(() => {
                if (movementComponent.IsOnFloor)
                {
                    actionsComponent.Land();
                }
                actionsComponent.Fall();
            });
        }

        protected override void AnimationUpdate() {}

        protected override void VelocityUpdate() {}
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
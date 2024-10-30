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


		public override void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationComponent anim, CharacterCollisionComponent col)
		{
			base.InitState(mc, cv, ac, anim, col);

            // Subscribe the SM Battle state to the Combate SM signlas for exiting/updating animations
            CombatStateMachine.CombateAnimationUpdate += OnBattleAnimationChange;
            CombatStateMachine.CombatEnd += OnBattleEnd;
		}

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnterInternal(configs);
            comboCounter = 0;

            // Subscribe Combat State machine to action requests
            actionsComponent.AttackActionStart += CombatStateMachine.BasicAttackRequested;

            // Subscribe Combat State machine to the animation component events
            animationComponent.AnimationStepChange += CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished += CombatStateMachine.OnAnimationStepFinished;

            // We should really use this to have more dynamic movement in attacks. Momentum and such
            movementComponent.Velocities[VelocityType.MainMovement] = Vector2.Zero;

            CombatStateMachine.ChangeAttackStep<BasicAttackStep>();
        }

        public override void StateExit()
        {
            animationComponent.AnimationStepChange -= CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished -= CombatStateMachine.OnAnimationStepFinished;

            actionsComponent.AttackActionStart -= CombatStateMachine.BasicAttackRequested;
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
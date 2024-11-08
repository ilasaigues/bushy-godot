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
        
		public override void InitState(MovementComponent mc, CharacterVariables cv, ActionsComponent ac, AnimationComponent anim, CharacterCollisionComponent col)
		{
			base.InitState(mc, cv, ac, anim, col);

            // Subscribe the SM Battle state to the Combate SM signlas for exiting/updating animations
            CombatStateMachine.CombateAnimationUpdate += OnBattleAnimationChange;
            CombatStateMachine.CombatEnd += OnBattleEnd;

            CombatStateMachine.CombatMovementUpdate += OnCombatMovementUpdate;
		}

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnterInternal(configs);
            
            // Subscribe Combat State machine to action requests
            actionsComponent.AttackActionStart += CombatStateMachine.BasicAttackRequested;
            actionsComponent.MovementDirectionChange += OnMovementDirectionChange;

            // Subscribe Combat State machine to the animation component events
            animationComponent.AnimationStepChange += CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished += CombatStateMachine.OnAnimationStepFinished;


            // We should really use this to have more dynamic movement in attacks. Momentum and such
            movementComponent.Velocities[VelocityType.MainMovement] = Vector2.Zero;

            var attackDirection = actionsComponent.MovementDirection == Vector2.Zero 
                ? movementComponent.FacingDirection
                : actionsComponent.MovementDirection;

            var attackConfig = new AttackStepConfig(attackDirection.Normalized());    
            CombatStateMachine.ChangeAttackStep<BasicAttackStep>(attackConfig);
        }

        public override void StateExit()
        {
            animationComponent.AnimationStepChange -= CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished -= CombatStateMachine.OnAnimationStepFinished;

            actionsComponent.AttackActionStart -= CombatStateMachine.BasicAttackRequested;

            actionsComponent.MovementDirectionChange -= OnMovementDirectionChange;
        }

        public override void StateUpdateInternal(double delta)
        {
            CombatStateMachine.CombatUpdate(delta);
            // Movement component changes
        } 

        public void OnBattleAnimationChange(string animationKey, Vector2 direction)
        {
            movementComponent.StartCoreography();
            movementComponent.CoreographFaceDirection(direction);
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

        public void OnMovementDirectionChange(Vector2 direction) 
        {
            if (direction.X != 0)
                CombatStateMachine.UpdateCombatStepConfigs(new AttackStepConfig(direction));
        }

        public void OnCombatMovementUpdate(Vector2 direction)
        {
            movementComponent.StartCoreography();
            movementComponent.Velocities[VelocityType.Locked] = direction;
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
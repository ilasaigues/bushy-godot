using System;
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

        private AxisMovement xAxisMovement;
        private AxisMovement yAxisMovement;
        private bool airborneCombat;
		public override void InitState(MovementComponent mc, CharacterVariables cv, PlayerActionsComponent ac, AnimationComponent anim, 
            CharacterCollisionComponent col, StateMachine sm)
		{
			base.InitState(mc, cv, ac, anim, col, sm);

            // Subscribe the SM Battle state to the Combate SM signlas for exiting/updating animations
            CombatStateMachine.CombateAnimationUpdate += OnBattleAnimationChange;
            CombatStateMachine.CombatEnd += OnBattleEnd;

            CombatStateMachine.CombatMovementUpdate += OnCombatMovementUpdate;
            CombatStateMachine.CombatAttackHit += OnCombatAttackHit;

            CombatStateMachine.InitMachine(mc);

            // Create axis components with no max speed that will always be aplying the intended acceleration
            this.xAxisMovement = new AxisMovement.Builder()
				.Acc(0).Dec(0).Speed(Int32.MaxValue).OverDec(0).TurnDec(0).Movement(mc)
				.Direction(() => { return 1; })
				.ColCheck((dir) => { return mc.IsOnWall; })
				.Variables(cv)
				.Build();
            this.yAxisMovement = this.xAxisMovement.ToBuilder().Copy()
				.ColCheck((dir) => { return dir > 0 ? mc.IsOnFloor : mc.IsOnCeiling; })
                .Build();
		}

        protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnterInternal(configs);

            this.airborneCombat = !movementComponent.IsOnFloor;
            
            // Subscribe Combat State machine to action requests
            actionsComponent.AttackActionStart += CombatStateMachine.BasicAttackRequested;
            actionsComponent.MovementDirectionChange += OnMovementDirectionChange;

            // Subscribe Combat State machine to the animation component events
            animationComponent.AnimationStepChange += CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished += CombatStateMachine.OnAnimationStepFinished;


            // // We should really use this to have more dynamic movement in attacks. Momentum and such
            // movementComponent.Velocities[VelocityType.MainMovement] = Vector2.Zero;

            // // We should really use this to have more dynamic movement in attacks. Momentum and such
            // movementComponent.Velocities[VelocityType.Gravity] = Vector2.Zero;

            var attackDirection = actionsComponent.MovementDirection == Vector2.Zero 
                ? movementComponent.FacingDirection
                : actionsComponent.MovementDirection;

            var attackConfig = new AttackStepConfig(attackDirection.Normalized());   

            if (movementComponent.IsOnFloor) 
                CombatStateMachine.ChangeAttackStep<BasicAttackStep>(attackConfig);
            else 
                CombatStateMachine.ChangeAttackStep<AirAttackStep>(attackConfig);
        }

        public override void StateExit()
        {
            animationComponent.AnimationStepChange -= CombatStateMachine.OnAnimationStepChange;
            animationComponent.AnimationFinished -= CombatStateMachine.OnAnimationStepFinished;

            actionsComponent.AttackActionStart -= CombatStateMachine.BasicAttackRequested;

            actionsComponent.MovementDirectionChange -= OnMovementDirectionChange;

            this.stateMachine.MachineState.X_AxisControlEnabled = true;
        }

        public override void StateUpdateInternal(double delta)
        {
            CombatStateMachine.CombatUpdate(delta);

            // Movement component changes
            xAxisMovement.HandleMovement(delta);
            yAxisMovement.HandleMovement(delta);
            Vector2 velocity = new Vector2((float)xAxisMovement.Velocity, (float)yAxisMovement.Velocity);
            movementComponent.Velocities[VelocityType.Locked] = velocity;

            this.stateMachine.MachineState.CurrentAnimationLevel = CascadePhaseConfig.AnimationLevel.UNINTERRUPTIBLE;
            this.stateMachine.MachineState.IsCommitedAction = true;
            this.stateMachine.MachineState.X_AxisControlEnabled = airborneCombat ? true : !movementComponent.IsOnFloor;
        } 

        public void OnBattleAnimationChange(string animationKey, Vector2 direction)
        {
            this.airborneCombat = !movementComponent.IsOnFloor;
            movementComponent.StartCoreography();
            movementComponent.CoreographFaceDirection(direction);
            animationComponent.Play(animationKey);
        }

        public void OnBattleEnd()
        {
            RunAndEndState(() => {
                actionsComponent.Idle(this.stateMachine);
            });
        }

        public void OnMovementDirectionChange(Vector2 direction) 
        {
            if (direction.X != 0)
                CombatStateMachine.UpdateCombatStepConfigs(new AttackStepConfig(direction));
        }

        public void OnCombatMovementUpdate(Vector2 velocity, Vector2 acceleration)
        {
            movementComponent.StartCoreography();
            xAxisMovement = xAxisMovement.ToBuilder().Vel(velocity.X).Acc((int) acceleration.X).Build();
            yAxisMovement = yAxisMovement.ToBuilder().Vel(velocity.Y).Acc((int) acceleration.Y).Build();
            
            movementComponent.Velocities[VelocityType.Locked] = velocity;
        }

        public void OnCombatAttackHit()
        {

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
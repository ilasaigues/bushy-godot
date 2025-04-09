using Godot;
using static BushyCore.StateConfig;

namespace BushyCore
{
    public partial class AttackParentState : BaseParentState<PlayerController, AttackParentState>
    {
        [Export] public Timer AttackCooldownTimer;
        [Export] public Timer ComboTimer;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {

        }

        public void SetFirstAttack(ComboAttackData attackData)
        {
            float attackCooldown = attackData.AttackTimer;
            float comboTime = attackData.NextComboTimer;
            AttackCooldownTimer.Start(attackCooldown);
            ComboTimer.Start(comboTime);
        }

        protected override void ExitStateInternal()
        {
            base.ExitStateInternal();
            AttackCooldownTimer.Stop();
            ComboTimer.Stop();
        }

        public bool TryPerformNextAttack(ComboAttackData attackData)
        {
            if (AttackCooldownTimer.IsStopped() && ComboTimer.TimeLeft > 0)
            {
                float attackCooldown = attackData.AttackTimer;
                float comboTime = attackData.NextComboTimer;
                AttackCooldownTimer.Start(attackCooldown);
                ComboTimer.Start(comboTime);

                return true;
            }
            return false;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (ComboTimer.IsStopped())
            {
                // Return to falling if in air, or idle if grounded
                if (Agent.MovementComponent.IsOnFloor)
                {
                    if (Agent.MovementInputVector.X == 0)
                    {
                        throw StateInterrupt.New<IdleGroundedState>(true);
                    }
                    else
                    {
                        throw StateInterrupt.New<WalkState>(true);
                    }
                }
                throw StateInterrupt.New<FallState>(true);
            }

            // process the current attack state (air or ground) and return the according executionstatus, like allowed movement and such
            prevStatus.MovementLockFlags |= MovementLockFlags.HorizontalLock;
            return ProcessSubState(prevStatus, delta);
        }
    }
}
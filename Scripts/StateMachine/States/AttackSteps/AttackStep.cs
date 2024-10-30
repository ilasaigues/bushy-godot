using System;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    abstract partial class AttackStep : Node2D
    {
        [Signal]
        public delegate void BattleAnimationChangeEventHandler(string animationKey);
        [Signal]
        public delegate void AttackStepCompletedEventHandler();
        [Signal]
        public delegate void ComboStepEventHandler(AttackStep attackStep); 

        [Export]
        public Animation animation;
        
        public void InitState() {}   
        public virtual void CombatUpdate(double delta) {}
        public virtual void StepEnter() {
            currentPhase = AttackStepPhase.WINDUP;
        }
        public virtual void StepExit() {}
        protected AttackStepPhase currentPhase;

        public void ChangePhase(int phase)
        {
            switch (currentPhase) {
                case AttackStepPhase.WINDUP:
                    currentPhase = AttackStepPhase.ACTION;
                    break;
                case AttackStepPhase.ACTION:
                    currentPhase = AttackStepPhase.RECOVERY;
                    break;
                case AttackStepPhase.RECOVERY:
                    StepExit();
                    EmitSignal(SignalName.AttackStepCompleted);
                break;
            }
        }

        protected enum AttackStepPhase 
        {
            WINDUP,
            ACTION,
            RECOVERY
        }

        public virtual void HandleAttackAction() {}

    }
}
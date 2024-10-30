using System.Collections.Generic;
using Godot;
using System.Linq;
using System;
using System.Diagnostics;

namespace BushyCore 
{
    partial class CombatStateMachine : Node2D
    {
        [Signal]
        public delegate void CombateAnimationUpdateEventHandler(string animation_key);
        [Signal]
        public delegate void CombatEndEventHandler();

        private Dictionary<Type, AttackStep> attackSteps;
        private AttackStep currentAttack;
        public override void _Ready()
		{	
			attackSteps = GetChildren()
				.Where(n => n is AttackStep)
				.Select(ats => {
					var attackStep = (AttackStep) ats;
					attackStep.InitState();
                    attackStep.AttackStepCompleted += EndStateMachine;
                    attackStep.BattleAnimationChange += UpdateCombatAnimation;
                    attackStep.ComboStep += ChangeAttackStep;
					return attackStep;
				})
				.ToDictionary(s => s.GetType());
			currentAttack = attackSteps.Values.First();
		}
        public void CombatUpdate(double delta) 
        {
            currentAttack.CombatUpdate(delta);
        }

        public void ChangeAttackStep<T>() where T: AttackStep 
        {
            if (attackSteps.ContainsKey(typeof(T)))
			{
				var nextState = (T)attackSteps[typeof(T)];
				currentAttack.StepExit();
				nextState.StepEnter();
				currentAttack = nextState;
                return;
			}
			throw new System.Exception($"No state instance of type {typeof(T)} found.");
        }

        public void ChangeAttackStep(AttackStep nextStep) 
        {
            if (attackSteps.Values.Contains(nextStep))
			{
				currentAttack.StepExit();
				nextStep.StepEnter();
				currentAttack = nextStep;
                return;
			}
			throw new System.Exception($"This attack step provided is not in the SM list of steps. {nextStep}.");
        }

        public void EndStateMachine() => EmitSignal(SignalName.CombatEnd);
        public void UpdateCombatAnimation(string animationKey) => EmitSignal(SignalName.CombateAnimationUpdate, animationKey);
        
        public void OnAnimationStepChange(int state) 
        {
            currentAttack.ChangePhase(state);
        }

        public void OnAnimationStepFinished(StringName animationKey)
        {
            currentAttack.ChangePhase(0);
        }

        public void BasicAttackRequested()
        {
            currentAttack.HandleAttackAction();
        }
    }
}
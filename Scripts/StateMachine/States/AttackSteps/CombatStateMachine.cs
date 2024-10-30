using System.Collections.Generic;
using Godot;
using System.Linq;
using System;

namespace BushyCore 
{
    partial class CombatStateMachine : Node2D
    {
        private Dictionary<Type, AttackStep> attackSteps;
        private AttackStep currentAttack;
        public override void _Ready()
		{	
			attackSteps = GetChildren()
				.Where(n => n is AttackStep)
				.Select(ats => {
					var attackStep = (AttackStep) ats;
					attackStep.InitState();
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

        }

    }
}
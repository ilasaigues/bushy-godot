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
        public delegate void CombateAnimationUpdateEventHandler(string animation_key, Vector2 direction);
        [Signal]
        public delegate void CombatMovementUpdateEventHandler(Vector2 velocity, Vector2 acceleration);
        [Signal]
        public delegate void CombatAttackHitEventHandler();
        [Signal]
        public delegate void CombatEndEventHandler();

        private Dictionary<Type, AttackStep> attackSteps;
        private AttackStep currentAttack;
        private MovementComponent MovementComponent;
        public override void _Ready()
        {
            attackSteps = GetChildren()
                .Where(n => n is AttackStep)
                .Select(ats =>
                {
                    var attackStep = (AttackStep)ats;
                    attackStep.InitState();
                    attackStep.AttackStepCompleted += EndStateMachine;
                    attackStep.BattleAnimationChange += UpdateCombatAnimation;
                    attackStep.ComboStep += ChangeAttackStep;
                    attackStep.ForceCoreography += HandleCoreography;
                    attackStep.HitboxComponent.HitboxImpact += OnAttackImpact;
                    return attackStep;
                })
                .ToDictionary(s => s.GetType());
            currentAttack = attackSteps.Values.First();
        }
        public void InitMachine(MovementComponent movementComponent)
        {
            MovementComponent = movementComponent;
            foreach (var step in attackSteps.Values)
            {
                step.MovementComponent = MovementComponent;
            }

        }
        public void CombatUpdate(double delta)
        {
            currentAttack.CombatUpdate(delta);
        }

        public void ChangeAttackStep<T>(AttackStepConfig config) where T : AttackStep
        {
            if (attackSteps.ContainsKey(typeof(T)))
            {
                var nextState = (T)attackSteps[typeof(T)];
                currentAttack.StepExit();
                nextState.StepEnter(config);
                currentAttack = nextState;
                return;
            }
            throw new Exception($"No state instance of type {typeof(T)} found.");
        }

        public void ChangeAttackStep(AttackStep nextStep, AttackStepConfig config)
        {
            if (attackSteps.Values.Contains(nextStep))
            {
                currentAttack.StepExit();
                nextStep.StepEnter(config);
                currentAttack = nextStep;
                return;
            }
            throw new Exception($"This attack step provided is not in the SM list of steps. {nextStep}.");
        }

        public void EndStateMachine()
        {
            currentAttack.StepExit();
            EmitSignal(SignalName.CombatEnd);
        }

        public void UpdateCombatStepConfigs(AttackStepConfig stepConfig) { currentAttack.HandleStepConfigChange(stepConfig); }
        public void UpdateCombatAnimation(string animationKey, Vector2 direction) => EmitSignal(SignalName.CombateAnimationUpdate, animationKey, direction);

        public void OnAnimationStepChange(int phase) => currentAttack.ChangePhase(phase);

        public void OnAnimationStepFinished(StringName _animationKey) { Debug.WriteLine("animation ended"); currentAttack.ChangePhase(4); }

        public void BasicAttackRequested() => currentAttack.HandleAttackAction();

        public void HandleCoreography(Vector2 velocity, Vector2 acceleration)
        {
            EmitSignal(SignalName.CombatMovementUpdate, velocity, acceleration);
        }

        public void OnAttackImpact() => EmitSignal(SignalName.CombatAttackHit);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    [Tool]
    public partial class AttackStep : Node2D
    {
        [Signal]
        public delegate void BattleAnimationChangeEventHandler(string animationKey, Vector2 direction);
        [Signal]
        public delegate void AttackStepCompletedEventHandler();
        [Signal]
        public delegate void ComboStepEventHandler(AttackStep attackStep, AttackStepConfig config); 
        [Signal]
        public delegate void ForceCoreographyEventHandler(Vector2 velocity, Vector2 acceleration);

        [Export]
        public string animationKey;
        private bool _DebugHitbox;
        [Export]
        public bool DebugHitbox { 
            get { return _DebugHitbox; }
            set { _DebugHitbox = value; QueueRedraw();}
        }

        [Export]
        protected virtual Shape2D hitboxShape {get;set;}
        [Export]
        protected Vector2 attackVector;
        [Export]
        protected Vector2 attackMovement;
        [Export]
        protected Godot.Collections.Array<PhaseCoreography> Coreographies;
        
        [Node]
        protected Timer CoreographyTimer;
        public HitboxComponent HitboxComponent;
        public MovementComponent MovementComponent;

        public override void _Ready()
        {
            this.AddToGroup();
            this.WireNodes();
            HitboxComponent = GetNode<HitboxComponent>("HitboxComponent");
            HitboxComponent.collisionShape2D.Shape = hitboxShape;
        }

        public void InitState() {
            CoreographyTimer.Timeout += OnCoreographyTimeout;
        }   

        public virtual void CombatUpdate(double delta) {}

        public virtual void StepEnter(AttackStepConfig config) {
            currentPhase = AttackStepPhase.WINDUP;
            attackStepConfigs = config;
            HitboxComponent.Position = attackVector * new Vector2(attackStepConfigs.Direction.X, 1).Normalized();

            EmitSignal(SignalName.BattleAnimationChange, animationKey, config.Direction);
            CoreographMovement();
        }

        public virtual void StepExit() {
            SpawnHitbox(false);
        }

        protected AttackStepPhase currentPhase;
        protected AttackStepConfig attackStepConfigs;

        public void ChangePhase(int phase)
        {
            var phasesCount = Enum.GetValues<AttackStepPhase>().Count();
            
            if (phase >= phasesCount)
            {
                EmitSignal(SignalName.AttackStepCompleted);
                return;
            } 

            if (phase <= (int) currentPhase) throw new Exception("Combat attack step phase change invalid transition");

            var phaseEnum = (AttackStepPhase) phase;
            currentPhase = phaseEnum;
            
            switch (currentPhase)
            {
                case AttackStepPhase.ACTION:
                    SpawnHitbox(true);
                    break;
                case AttackStepPhase.RECOVERY:
                    SpawnHitbox(false);
                    break;
                default:
                    break;
            }

            CoreographMovement();
        }
        protected virtual void CoreographMovement() {
            var coreography = Coreographies.FirstOrDefault(x => x.Phase == currentPhase, null);

            if (coreography == null) return;

            if (coreography.BeginOnTimerEnd) {
                return;
            }

            CoreographyTimer.WaitTime = coreography.TimerDuration;
            CoreographyTimer.Start();
            
            EmitSignal(SignalName.ForceCoreography, 
                coreography.VelocityVector * attackStepConfigs.Direction.Normalized(),
                coreography.AccelerationVector * attackStepConfigs.Direction.Normalized()
            );
        }

        protected void OnCoreographyTimeout()
        {
            EmitSignal(SignalName.ForceCoreography, Vector2.Zero, Vector2.Zero);
        }

        protected virtual void SpawnHitbox(bool enable) {
            HitboxComponent.ToggleEnable(enable);
        }

        public void HandleStepConfigChange(AttackStepConfig configs) { this.attackStepConfigs = configs; }

        public enum AttackStepPhase 
        {
            WINDUP,
            ACTION,
            COMBO,
            RECOVERY
        }

        public virtual void HandleAttackAction() {}

        public override void _Draw()
        {
            if (!Engine.IsEditorHint())
                return;
            
            if (hitboxShape == null) 
                return;

            if (DebugHitbox)
                DrawRect(hitboxShape.GetRect(), new Godot.Color("#f528914d"));
            
            base._Draw();
        }

        public override void _ExitTree()
        {
            hitboxShape = null;
            QueueRedraw();
            base._ExitTree();
        }
    }
}
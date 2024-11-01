using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Godot;
using GodotUtilities;

namespace BushyCore
{
    [Scene]
    [Tool]
    partial class AttackStep : Node2D
    {
        [Signal]
        public delegate void BattleAnimationChangeEventHandler(string animationKey);
        [Signal]
        public delegate void AttackStepCompletedEventHandler();
        [Signal]
        public delegate void ComboStepEventHandler(AttackStep attackStep, AttackStepConfig config); 

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
        
        public HitboxComponent HitboxComponent;

        public override void _Ready()
        {
            HitboxComponent = GetNode<HitboxComponent>("HitboxComponent");
            HitboxComponent.collisionShape2D.Shape = hitboxShape;
        }

        public void InitState() {}   
        public virtual void CombatUpdate(double delta) {}
        public virtual void StepEnter(AttackStepConfig config) {
            currentPhase = AttackStepPhase.WINDUP;
            attackStepConfigs = config;
            EmitSignal(SignalName.BattleAnimationChange, animationKey);
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
        }

        protected virtual void SpawnHitbox(bool enable) {
            HitboxComponent.ToggleEnable(enable);
            if (enable)
                HitboxComponent.Position = attackVector * Vector2.One * attackStepConfigs.Direction.Normalized();
            
        }

        protected enum AttackStepPhase 
        {
            WINDUP,
            ACTION,
            COMBO,
            RECOVERY
        }

        public virtual void HandleAttackAction() {}

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
            {
                this.AddToGroup();
                this.WireNodes();
            }
        }

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
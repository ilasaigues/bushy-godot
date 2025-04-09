using System;
using Godot;

namespace BushyCore
{
    public abstract partial class BaseAttackChildState : BaseChildState<PlayerController, AttackParentState>
    {
        [Export] protected ComboData _comboData;
        protected int _currentComboStep;

        public override void EnterState(Type type, params StateConfig.IBaseStateConfig[] configs)
        {
            base.EnterState(type, configs);
            ParentState.SetFirstAttack(_comboData.Attacks[0]);
        }

        protected void TryAttack()
        {
            if (_comboData.Loop)
            {
                if (ParentState.TryPerformNextAttack(_comboData.Attacks[_currentComboStep % _comboData.Attacks.Length]))
                {
                    Agent.AnimController.SetTrigger(PlayerController.AnimConditions.AttackTrigger);
                    _currentComboStep++;
                }
            }
            else
            {
                if (_currentComboStep < _comboData.Attacks.Length - 1 &&
                ParentState.TryPerformNextAttack(_comboData.Attacks[_currentComboStep]))
                {
                    Agent.AnimController.SetTrigger(PlayerController.AnimConditions.AttackTrigger);
                    _currentComboStep++;
                }
            }
        }

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            Agent.AnimController.SetTrigger(PlayerController.AnimConditions.AttackTrigger);
        }

        protected override void ExitStateInternal()
        {
            _currentComboStep = 0;
        }

        protected override bool OnInputButtonChangedInternal(InputAction.InputActionType actionType, InputAction Action)
        {
            if (actionType == InputAction.InputActionType.InputPressed && Action == InputManager.Instance.AttackAction)
            {
                TryAttack();
            }
            return false;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (!_comboData.GravityEnabled)
            {
                prevStatus.MovementLockFlags |= MovementLockFlags.VerticalLock;
            }
            prevStatus.AnimationLevel |= StateAnimationLevel.Uninterruptible;
            return prevStatus;
        }
    }
}
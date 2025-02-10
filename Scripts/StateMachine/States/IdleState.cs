using BushyCore;
using Godot;
using System;
using System.Diagnostics;

namespace BushyCore
{
	public partial class IdleState : BaseState
	{
		protected override void StateEnterInternal(params StateConfig.IBaseStateConfig[] configs)
		{
			actionsComponent.AttackActionStart += AttackActionRequested;
		}

        public override void StateExit()
        {
			actionsComponent.AttackActionStart -= AttackActionRequested;
            base.StateExit();
        }
		public void AttackActionRequested()
		{
			Debug.WriteLine("Requesting attack");
			RunAndEndState(() => actionsComponent.MainAttack(this.stateMachine));
		}
		protected override void VelocityUpdate()
		{
		}

		protected override void AnimationUpdate()
		{
		}
	}

}

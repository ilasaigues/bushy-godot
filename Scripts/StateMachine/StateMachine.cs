using Godot;
using GodotUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BushyCore
{
	public partial class StateMachine : Node
	{
		[Export]
		private MovementComponent movementComponent;
		[Export]
		private CharacterVariables characterVariables;
		[Export]
		private ActionsComponent actionsComponent;

		private Dictionary<System.Type, BaseState> states;
		private BaseState currentState;
		public override void _Ready()
		{	
			actionsComponent.SetStateMachine(this);
			states = GetChildren()
				.Where(n => n is BaseState)
				.Select(bs => {
					var baseState = (BaseState) bs;
					baseState.InitState(movementComponent, characterVariables, actionsComponent);
					return baseState;
				})
				.ToDictionary(s => s.GetType());

			currentState = states.Values.First();
		}

		public override void _Notification(int what)
		{
			
		}

		public override void _Process(double delta)
		{
			currentState.StateUpdate(delta);
		}
		
		public void ChangeState<T>(params StateConfig.IBaseStateConfig[] configs) where T : BaseState
		{
			if (states.ContainsKey(typeof(T)))
			{
				var nextState = (T)states[typeof(T)];
				currentState.StateExit();
				var previousState = currentState;
				nextState.StateEnter(configs);
				currentState = nextState;
				throw new StateEndedException(previousState);
			}
			throw new System.Exception($"No state instance of type {typeof(T)} found.");
		}
	}
}


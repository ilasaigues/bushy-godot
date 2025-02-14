using Godot;
using GodotUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BushyCore
{
	public partial class StateMachine : Node2D
	{
		[Export]
		private MovementComponent movementComponent;
		[Export]
		private CharacterVariables characterVariables;
		[Export]
		private PlayerActionsComponent actionsComponent;
		[Export]
		private AnimationComponent animationComponent;
		[Export]
		private CharacterCollisionComponent collisionComponent;

		public CascadePhaseConfig MachineState { get; set; } 
		private Dictionary<System.Type, BaseState> states;
		private BaseState currentState;
		public override void _Ready()
		{	
			states = GetChildren()
				.Where(n => n is BaseState)
				.Select(bs => {
					var baseState = (BaseState) bs;
					baseState.InitState(movementComponent, characterVariables, actionsComponent, animationComponent, collisionComponent, this);
					return baseState;
				})
				.ToDictionary(s => s.GetType());

			currentState = states.Values.First();
			GD.Print(currentState.GetName());
			currentState.StateEnter();
		}

		public void MachineProcess(double delta)
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


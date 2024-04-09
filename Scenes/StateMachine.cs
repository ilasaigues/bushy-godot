using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class StateMachine : Node
{
	private MovementComponent movementComponent;
	
	private ICollection<BaseState> states;
	private BaseState currentState;
	public override void _Ready()
	{	
		this.movementComponent = GetParent().GetNode<MovementComponent>("MovementComponent");
		states = GetChildren().Where(c => c is BaseState).Select(s => (BaseState) s).ToArray();
		
 		currentState = states.ElementAt(0);
		foreach (BaseState state in states) state.InitState(movementComponent);
	}
	public override void _Process(double delta)
	{
		Debug.WriteLine("PROCESSSSSSSSSSS");
		currentState.UpdateInternal(delta);
	}
}

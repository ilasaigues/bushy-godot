using BushyCore;
using Godot;
using System;

// The main idea behind this is to abstract how a StateMachine will obtain commands to transition, and also to mock it's inputs
// In the future, we can use this to inject behavior into an enemy's state machine, or any other npc. This base class is meant
// to be as generic as possible and will only have basic movement logic. We can assume most state machines will have states for
// Ground, Jump, Fall and possibly dash. Since all these can be modified via CharacterVariables, we can reuse this logic
// Input Managers will update the ACtionsComponent
public partial class ActionsComponent : Node
{
	public bool CanJump { get; set; }
	public bool CanDash { get; set; }

	public Vector2 MovementDirection { get; set; }
	public bool IsJumpRequested { get; set; }
	public bool IsDashRequested { get; set; }

	private StateMachine stateMachine;

	public void SetStateMachine(StateMachine sm) 
	{
		this.stateMachine = sm;
	}

	// State machine changes WILL throw state end exceptions. After this method is called, state will end immediately 
	public void Jump(params StateConfig.IBaseStateConfig[] configs)
	{
		this.stateMachine.ChangeState<JumpState>(configs);
	}
	// State machine changes WILL throw state end exceptions. After this method is called, state will end immediately
	public void Fall()
	{
		this.stateMachine.ChangeState<AirState>(StateConfig.InitialVerticalVelocity(0));
	}
	
}

using Godot;
using System;
using GodotUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BushyCore
{
	[Scene]
	public partial class PlayerController : CharacterBody2D
	{
		[Node]
		public MovementComponent MovementComponent;
		[Node]
		public CharacterCollisionComponent CollisionComponent;

		[Export]
		public CharacterVariables CharacterVariables;

		[Export]
		public AnimationComponent AnimationComponent;

		public PlayerInfo PlayerInfo = new PlayerInfo();

		private Vector2 _movementInputVector;
		public Vector2 MovementInputVector => _movementInputVector.Normalized();

		private InputManager inputManager => InputManager.Instance;

		[Export]
		private BaseState<PlayerController>[] BaseStates;
		[Export]
		private BaseState<PlayerController>[] ActionStates;
		[Export]
		private BaseState<PlayerController>[] OverrideStates;

		private StateMachine<PlayerController> _baseStateMachine;
		private StateMachine<PlayerController> _actionStateMachine;
		private StateMachine<PlayerController> _overrideStateMachine;

		private StateMachine<PlayerController>[] _stateMachineStack;

		private List<DisposableBinding> _disposableBindings = new();

		public override void _Notification(int what)
		{
			if (what == NotificationSceneInstantiated)
			{
				this.AddToGroup();
				this.WireNodes();
			}
		}

		public override void _Ready()
		{
			base._Ready();
			_baseStateMachine = new StateMachine<PlayerController>(this);
			_actionStateMachine = new StateMachine<PlayerController>(this);
			_overrideStateMachine = new StateMachine<PlayerController>(this);
			foreach (var state in BaseStates)
			{
				_baseStateMachine.RegisterState(state);
			}
			foreach (var state in ActionStates)
			{
				_actionStateMachine.RegisterState(state);
			}
			foreach (var state in OverrideStates)
			{
				_overrideStateMachine.RegisterState(state);
			}

			_stateMachineStack = new StateMachine<PlayerController>[]
				{
					_overrideStateMachine,
					_actionStateMachine,
					_baseStateMachine,
				};
			MovementComponent.SetParentController(this);
			CollisionComponent.SetParentController(this);
			BindInputs();
			_baseStateMachine.SetState(typeof(FallState));
		}

		void BindInput(DisposableBinding binding)
		{
			_disposableBindings.Add(binding);
		}

		void BindInputs()
		{

			var im = InputManager.Instance;
			BindInput(im.HorizontalAxis.BindToAxisUpdated(OnHorizontalAxisChanged));
			BindInput(im.VerticalAxis.BindToAxisUpdated(OnVerticalAxisChanged));

			foreach (var inputAction in im.InputActions)
			{
				BindInput(inputAction.BindToInputHeld(
					(a) =>
					{
						TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputHeld, a));
					}));
				BindInput(inputAction.BindToInputJustPressed(
					(a) =>
					{
						TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputPressed, a));
					}));
				BindInput(inputAction.BindToInputReleased(
					(a) =>
					{
						TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputReleased, a));
					}));
			}
			BindInput(im.HorizontalAxis.BindToAxisUpdated(
					(a) =>
					{
						TryToChain(sm => sm.UpdateInputAxis(a));
					}));
			BindInput(im.VerticalAxis.BindToAxisUpdated(
					(a) =>
					{
						TryToChain(sm => sm.UpdateInputAxis(a));
					}));

		}

		private void TryToChain(Func<StateMachine<PlayerController>, bool> handler)
		{
			TryOrCascade(() =>
			{
				if (handler(_overrideStateMachine))
				{
					if (handler(_actionStateMachine))
					{
						handler(_baseStateMachine);
					}
				}
			});
		}

		private void OnHorizontalAxisChanged(InputAxis horizontalAxis)
		{
			_movementInputVector.X = horizontalAxis.Value;
		}

		private void OnVerticalAxisChanged(InputAxis verticalAxis)
		{
			_movementInputVector.Y = verticalAxis.Value;
		}

		private void TryOrCascade(Action handler)
		{
			try
			{
				handler();
			}
			catch (StateInterrupt interrupt)
			{
				CascadeThroughStates(interrupt.NextStateType, interrupt.Configs);
			}
		}

		private void CascadeThroughStates(Type stateType, params StateConfig.IBaseStateConfig[] configs)
		{
			GD.Print("CASCADING");
			if (!_overrideStateMachine.SetState(stateType, configs))
			{
				if (!_actionStateMachine.SetState(stateType, configs))
				{
					if (!_baseStateMachine.SetState(stateType, configs))
					{
						throw new Exception($"Type {stateType} is not a state in any player state machine!");
					}
				}
			}
		}

		void ClearInputBindings()
		{
			_disposableBindings.ForEach(b => b.Dispose());
			_disposableBindings.Clear();
		}

		public override void _ExitTree()
		{
			foreach (var sm in _stateMachineStack)
			{
				ClearInputBindings();
			}
			_disposableBindings.ForEach(d => d.Dispose());
			base._ExitTree();
		}

		public override void _Process(double delta)
		{
			try
			{
				var stateResult = _overrideStateMachine.ProcessState(default, delta);
				stateResult = _actionStateMachine.ProcessState(stateResult, delta);
				_baseStateMachine.ProcessState(stateResult, delta);
			}
			catch (StateInterrupt interrupt)
			{
				CascadeThroughStates(interrupt.NextStateType, interrupt.Configs);
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			MovementComponent.UpdateState(this);
			MovementComponent.Move(this);
		}
	}

}

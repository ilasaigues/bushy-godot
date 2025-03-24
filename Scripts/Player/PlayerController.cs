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
		public PlayerCascadeStateMachine CascadeStateMachine;
		[Export]
		public AnimationComponent AnimationComponent;
		[Export]
		private SpriteTrail spriteTrail;
		public PlayerInfo PlayerInfo;

		private Vector2 _movementInputVector;
		public Vector2 MovementInputVector => _movementInputVector.Normalized();

		private InputManager inputManager => InputManager.Instance;

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
			PlayerInfo = new PlayerInfo(CharacterVariables);
			MovementComponent.SetParentController(this);
			CollisionComponent.SetParentController(this);
			BindInputs();
			CascadeStateMachine.SetAgent(this);
			CascadeStateMachine.SetState(typeof(FallState));
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
						CascadeStateMachine.TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputHeld, a));
					}));
				BindInput(inputAction.BindToInputJustPressed(
					(a) =>
					{
						CascadeStateMachine.TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputPressed, a));
					}));
				BindInput(inputAction.BindToInputReleased(
					(a) =>
					{
						CascadeStateMachine.TryToChain(sm => sm.UpdateStateInput(InputAction.InputActionType.InputReleased, a));
					}));
			}
			BindInput(im.HorizontalAxis.BindToAxisUpdated(
					(a) =>
					{
						CascadeStateMachine.TryToChain(sm => sm.UpdateInputAxis(a));
					}));
			BindInput(im.VerticalAxis.BindToAxisUpdated(
					(a) =>
					{
						CascadeStateMachine.TryToChain(sm => sm.UpdateInputAxis(a));
					}));

		}

		private void OnHorizontalAxisChanged(InputAxis horizontalAxis)
		{
			_movementInputVector.X = horizontalAxis.Value;
		}

		private void OnVerticalAxisChanged(InputAxis verticalAxis)
		{
			_movementInputVector.Y = verticalAxis.Value;
		}

		void ClearInputBindings()
		{
			_disposableBindings.ForEach(b => b.Dispose());
			_disposableBindings.Clear();
		}

		public override void _ExitTree()
		{

			ClearInputBindings();
			_disposableBindings.ForEach(d => d.Dispose());
			base._ExitTree();
		}

		public override void _PhysicsProcess(double delta)
		{

			MovementComponent.UpdateState(this);
			MovementComponent.Move(this);
		}
	}

}

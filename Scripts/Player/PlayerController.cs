using Godot;
using System;
using GodotUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BushyCore
{
	[Scene]
	public partial class PlayerController : CharacterBody2D
	{
		public static class AnimConditions
		{
			public const string Grounded = "parameters/PlayerMainLoop/conditions/Grounded";
			public const string OnAir = "parameters/PlayerMainLoop/conditions/OnAir";
			public const string Dashing = "parameters/PlayerMainLoop/conditions/Dashing";
			public const string Running = "parameters/PlayerMainLoop/Grounded/conditions/Running";
			public const string Falling = "parameters/PlayerMainLoop/AirState/conditions/Falling";
			public const string Hedge = "parameters/conditions/Hedge";
			public const string LandTrigger = "LandTrigger";
			public const string TurnTrigger = "TurnTrigger";
			public const string JumpTrigger = "JumpTrigger";
			public const string AttackTrigger = "AttackTrigger";
			public const string UpAttackTrigger = "UpAttackTrigger";
			public const string DownAttackTrigger = "DownAttackTrigger";
			public const string BushBlendValues = "parameters/BushBlendSpace/blend_position";
		}


		[Node] public MovementComponent MovementComponent;
		[Node] public CharacterCollisionComponent CollisionComponent;
		[Export] public CharacterVariables CharacterVariables;
		[Export] public PlayerCascadeStateMachine CascadeStateMachine;
		[Export] public AnimationPlayer AnimationPlayer;
		[Export] public AnimationController AnimController;
		[Export] public Sprite2DComponent Sprite2DComponent;
		[Export] private SpriteTrail spriteTrail;
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
				WireNodes();
			}
		}

		public override void _Ready()
		{
			base._Ready();
			AnimController.InitializeFromType(typeof(AnimConditions));
			PlayerInfo = new PlayerInfo(CharacterVariables);
			MovementComponent.SetParentController(this);
			CollisionComponent.SetParentController(this);
			BindInputs();
			CascadeStateMachine.SetAgent(this);
			CascadeStateMachine.SetState(typeof(FallState));
			//Engine.TimeScale = 0.1f;
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

		public void OnArea2DEnter(Area2D areaNode)
		{
			CascadeStateMachine.OnArea2DEnter(areaNode);
		}

		public void OnArea2DExit(Area2D areaNode)
		{
			CascadeStateMachine.OnArea2DExit(areaNode);
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

		public override void _Process(double delta)
		{
			Sprite2DComponent.ForceOrientation(new Vector2(MovementInputVector.X, 0));
		}


		public override void _PhysicsProcess(double delta)
		{

			MovementComponent.UpdateState(this);
			MovementComponent.Move(this);
		}
	}

}

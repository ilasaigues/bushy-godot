using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BushyCore
{
	public partial class InputsComponent : Node
	{
		[Export]
		private ActionsComponent actionsComponent;

		[Export]
		private float JumpBufferTime;
		[Export]
		private float DashBufferTime;
		[Export]
		private float AttackBufferTime;

		public override void _Ready()
		{
			InputManager.Instance.HorizontalAxis.OnAxisUpdated += OnHorizontalAxisChanged;
			InputManager.Instance.VerticalAxis.OnAxisUpdated += OnVerticalAxisChanged;
			InputManager.Instance.DashAction.OnInputJustPressed += OnDashRequested;
			InputManager.Instance.JumpAction.OnInputJustPressed += OnJumpRequested;
			InputManager.Instance.JumpAction.OnInputReleased += OnJumpReleased;
			InputManager.Instance.AttackAction.OnInputJustPressed += OnAttackRequested;
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			InputManager.Instance.HorizontalAxis.OnAxisUpdated -= OnHorizontalAxisChanged;
			InputManager.Instance.VerticalAxis.OnAxisUpdated -= OnVerticalAxisChanged;
			InputManager.Instance.DashAction.OnInputJustPressed -= OnDashRequested;
			InputManager.Instance.DashAction.OnInputReleased -= OnDashReleased;
			InputManager.Instance.JumpAction.OnInputJustPressed -= OnJumpRequested;
			InputManager.Instance.JumpAction.OnInputReleased -= OnJumpReleased;
			InputManager.Instance.AttackAction.OnInputJustPressed += OnAttackRequested;
		}

		private void OnDashRequested() 
		{
			actionsComponent.IsDashRequested = true;
		}
		private void OnDashReleased()
		{
			actionsComponent.IsDashCancelled = true;
		}
		private void OnAttackRequested()
		{
			actionsComponent.IsAttackRequested = true;
		}
		private void AttackCheck()
		{
			if (InputManager.Instance.AttackAction.TimePressed > AttackBufferTime)
			{
				actionsComponent.IsAttackRequested = false;
			}
		}

		private void DashCheck()
		{
			if (InputManager.Instance.DashAction.TimePressed > DashBufferTime)
			{
				actionsComponent.IsDashRequested = false;
			}
		}

		private void OnJumpRequested()
		{
			actionsComponent.IsJumpRequested = true;
			actionsComponent.IsJumpCancelled = false;
		}
		private void OnJumpReleased()
		{
			actionsComponent.IsJumpCancelled = true;
			actionsComponent.IsJumpRequested = false;
		}
		private void JumpCheck()
		{
			if (InputManager.Instance.JumpAction.TimePressed > JumpBufferTime)
			{
				actionsComponent.IsJumpRequested = false;
			}
		}

		// VERY IMPORTANT: the order in which these checks are done, impact the order of which action signals are fired
		public override void _Process(double delta)
		{
			this.DashCheck();
			this.JumpCheck();
		}

		private void OnHorizontalAxisChanged(float value)
		{
			actionsComponent.MovementDirection = new Vector2(value, actionsComponent.MovementDirection.Y);
		}

		private void OnVerticalAxisChanged(float value)
		{
			actionsComponent.MovementDirection = new Vector2(actionsComponent.MovementDirection.X, value);
		}
	}

}

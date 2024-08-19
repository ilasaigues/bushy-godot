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

		private double JumpInputTime;
		private double DashnputTime;

		public bool IsJumpRequested => (Time.GetTicksMsec() - JumpInputTime) <= JumpBufferTime;
		public bool IsDashRequested => (Time.GetTicksMsec() - DashnputTime) <= DashBufferTime;

		public override void _Ready()
		{
			InputManager.Instance.HorizontalAxis.OnAxisUpdated += OnHorizontalAxisChanged;
			InputManager.Instance.VerticalAxis.OnAxisUpdated += OnVerticalAxisChanged;
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			InputManager.Instance.HorizontalAxis.OnAxisUpdated -= OnHorizontalAxisChanged;
			InputManager.Instance.VerticalAxis.OnAxisUpdated -= OnVerticalAxisChanged;
		}

		// VERY IMPORTANT: the order in which these checks are done, impact the order of which action signals are fired
		public override void _Process(double delta)
		{
			this.DashCheck();
			this.JumpCheck();
		}
		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event.IsActionReleased("ui_accept"))
			{
				actionsComponent.IsJumpCancelled = true;
			}
		}

		private void OnHorizontalAxisChanged(float value)
		{
			actionsComponent.MovementDirection = new Vector2(value, actionsComponent.MovementDirection.Y);
		}

		private void OnVerticalAxisChanged(float value)
		{
			actionsComponent.MovementDirection = new Vector2(actionsComponent.MovementDirection.X, value);
		}

		private void JumpCheck()
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				this.JumpInputTime = Time.GetTicksMsec();
				actionsComponent.IsJumpRequested = true;
				actionsComponent.IsJumpCancelled = false;
			}

			if (this.JumpInputTime > 0 && !IsJumpRequested)
			{
				actionsComponent.IsJumpRequested = false;
				this.JumpInputTime = 0;
			}
		}
		private void DashCheck()
		{
			if (Input.IsActionJustPressed("left_shift"))
			{
				this.DashnputTime = Time.GetTicksMsec();
				actionsComponent.IsDashRequested = true;
			}

			if (this.DashnputTime > 0 && !IsDashRequested)
			{
				this.DashnputTime = 0;
				actionsComponent.IsDashRequested = false;
			}
		}

		private bool DashHeldCheck()
		{
			return Input.IsActionPressed("left_shift");
		}
	}

}

using Godot;
using System;
using System.Diagnostics;

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

		// VERY IMPORTANT: the order in which these checks are done, impact the order of which action signals are fired
        public override void _Process(double delta)
        {
			this.DirectionCheck();
			this.DashCheck();
			this.JumpCheck();
        }
        public override void _Input(InputEvent @event)
        {
			if (@event.IsActionReleased("ui_accept"))
			{
				actionsComponent.IsJumpCancelled = true;
			}
        }
        private void DirectionCheck()
		{
			bool anyDirJustChanged = Input.IsActionJustPressed("ui_left") ||
				Input.IsActionJustPressed("ui_right") ||
				Input.IsActionJustPressed("ui_up") ||
				Input.IsActionJustPressed("ui_down") ||
				Input.IsActionJustReleased("ui_left") ||
				Input.IsActionJustReleased("ui_right") ||
				Input.IsActionJustReleased("ui_up") ||
				Input.IsActionJustReleased("ui_down");

			if (anyDirJustChanged)
			{
				actionsComponent.MovementDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			}
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

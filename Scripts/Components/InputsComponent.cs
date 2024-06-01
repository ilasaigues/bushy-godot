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

		public bool IsJumpRequested => Input.IsActionPressed("ui_accept") && (Time.GetTicksMsec() - JumpInputTime) <= JumpBufferTime;
		public bool IsDashRequested => Input.IsActionPressed("left_shift") && (Time.GetTicksMsec() - DashnputTime) <= DashBufferTime;

        public override void _Process(double delta)
        {
			this.JumpCheck();
			this.DashCheck();
			actionsComponent.MovementDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        }

		private void JumpCheck()
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				this.JumpInputTime = Time.GetTicksMsec();
				actionsComponent.IsJumpRequested = true;
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

using Godot;
using System;
using System.Diagnostics;

namespace BushyCore 
{
	public partial class InputsComponent : Node
	{
		[Export]
		private ActionsComponent actionsComponent;

        public override void _Process(double delta)
        {
			if (Input.IsActionPressed("ui_accept"))
				actionsComponent.IsJumpRequested = true;
			else
				actionsComponent.IsJumpRequested = false;

			actionsComponent.MovementDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        }
	}

}

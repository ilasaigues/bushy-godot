using Godot;
using System;
using System.Linq;
using GodotUtilities;
using System.Collections.Generic;

namespace BushyCore
{
	public partial class CascadeMachine : Node2D
	{
		/*private List<PlayerStateController> cascadeLevels;
		public override void _Ready()
		{
			cascadeLevels = GetChildren()
				.Where(n => n is PlayerStateController)
				.Select(n => (PlayerStateController)n)
				.ToList();
		}
		public override void _Process(double deltaTime)
		{
			var stateData = new StateExecutionState();

			foreach (var stateMachine in cascadeLevels)
			{
				stateMachine.MachineState = stateData;
				stateMachine.process(deltaTime);
			}
		}*/
	}
}


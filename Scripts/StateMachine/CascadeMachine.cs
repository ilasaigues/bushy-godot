using Godot;
using System;
using System.Linq;
using GodotUtilities;
using System.Collections.Generic;

namespace BushyCore
{
	public partial class CascadeMachine : Node2D
	{
		private List<StateMachine> cascadeLevels;
		public override void _Ready()
		{	
			cascadeLevels = GetChildren()
				.Where(n => n is StateMachine)
				.Select(n => (StateMachine) n)
				.ToList();
		}
		public override void _Process(double deltaTime)
		{
			var phaseConfig = new CascadePhaseConfig();

			foreach (var stateMachine in cascadeLevels)
			{
				stateMachine.MachineState = phaseConfig;
				stateMachine.MachineProcess(deltaTime);
			}
		}
	}
}


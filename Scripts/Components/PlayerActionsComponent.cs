using Godot;
using System;

public partial class PlayerActionsComponent : ActionsComponent
{
	
	[Signal]
	public delegate void BurstActionStartEventHandler();
    [Signal]
	public delegate void HarpoonActionStartEventHandler();
	private bool _IsBurstRequested;
	public bool IsBurstRequested {
		get { return _IsBurstRequested; } 
		set {
			_IsBurstRequested = value;
			
			if (_IsBurstRequested) 
				EmitSignal(SignalName.BurstActionStart);
		} 
	}

	
	private bool _IsHarpoonRequested;
	public bool IsHarpoonRequested {
		get { return _IsHarpoonRequested; } 
		set {
			_IsHarpoonRequested = value;
			
			if (_IsHarpoonRequested) 
				EmitSignal(SignalName.HarpoonActionStart);
		} 
	}
}

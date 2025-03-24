using BushyCore;
using Godot;
using System;

public class PlayerInfo
{
	readonly CharacterVariables _charVars;
	public bool CanJump;
	public bool DashEnabled;
	public bool DashInCooldown => (Time.GetTicksMsec() - LastDashTime) < _charVars.DashCooldown;
	public bool CanDash => DashEnabled && !DashInCooldown;
	public bool IsInDashMode;
	public double LastDashTime;
	private bool _lookingRight = true;
	public int LookDirection
	{
		get
		{
			return _lookingRight ? 1 : -1;
		}
		set
		{
			if (value > 0)
			{
				_lookingRight = true;
			}
			else if (value < 0)
			{
				_lookingRight = false;
			}
		}
	}
	public bool CanCoyoteJump;
	public bool CanFallIntoHedge;
	public PlayerInfo(CharacterVariables variables)
	{
		_charVars = variables;
	}
}

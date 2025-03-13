using Godot;
using System;

public class PlayerInfo
{
	public bool CanJump;
	public bool CanDash;
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
}

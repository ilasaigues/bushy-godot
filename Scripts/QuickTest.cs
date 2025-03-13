using Godot;
using System;

public partial class QuickTest : Node
{
    /*public override void _Ready()
    {
        base._Ready();
        InputManager.Instance.JumpAction.OnInputJustPressed += SpaceJustPressed;
        InputManager.Instance.JumpAction.WhileInputPressed += WhileSpacePressed;
        InputManager.Instance.JumpAction.OnInputReleased += SpaceReleased;
    }

    private void SpaceJustPressed()
    {
        GD.Print("Space Press Start");
    }

    private void WhileSpacePressed()
    {
        var extra = "";
        if (InputManager.Instance.JumpAction.TimeHeld <= 1)
        {
            extra = ". First Second";
        }
        GD.Print("Pressing Space. FPS: " + Engine.GetFramesPerSecond() + extra);
    }

    private void SpaceReleased()
    {
        GD.Print("Space Press End");
    }*/

}

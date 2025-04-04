using BushyCore;
using Godot;
using GodotUtilities;
using System;
using System.Linq;

public partial class CascadeMachineReader : RichTextLabel
{
    [Export] PlayerController player;

    PlayerCascadeStateMachine cascadeMachine;
    public override void _Ready()
    {
        cascadeMachine = player.GetChildren().OfType<PlayerCascadeStateMachine>().First();
    }


    public override void _Process(double delta)
    {
        string newText = "";
        foreach (var machine in cascadeMachine.StateMachines)
        {
            newText += machine.Name + ": " + machine.GetCurrentStateName() + "\n";
        }
        Text = newText;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BushyCore
{
    public abstract partial class CascadeStateMachine<TAgent> : Node where TAgent : Node
    {
        TAgent Agent;
        List<StateMachine<TAgent>> StateMachines;
        public override void _Ready()
        {
            StateMachines = GetChildren().OfType<StateMachine<TAgent>>().ToList();
        }

        public void TryToChain(Func<StateMachine<TAgent>, bool> handler)
        {
            TryOrCascade(() =>
            {
                foreach (var stateMachine in StateMachines)
                {
                    if (!handler(stateMachine))
                    {
                        break;
                    }
                }
            });
        }

        private void TryOrCascade(Action handler)
        {
            try
            {
                handler();
            }
            catch (StateInterrupt interrupt)
            {
                CascadeThroughStates(interrupt.NextStateType, interrupt.Configs);
            }
        }

        public void SetAgent(TAgent agent)
        {
            Agent = agent;
            foreach (var stateMachine in StateMachines)
            {
                stateMachine.SetAgent(Agent);
            }
        }

        public void SetState(Type stateType)
        {
            CascadeThroughStates(stateType);
        }

        private void CascadeThroughStates(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            GD.Print("CASCADING");
            foreach (var machine in StateMachines)
            {
                if (machine.SetState(stateType, configs))
                {
                    return;
                }
            }
            throw new Exception($"Type {stateType} is not a state in any player state machine!");
        }
        public override void _PhysicsProcess(double delta)
        {
            try
            {
                var stateresult = default(StateExecutionStatus);
                foreach (var machine in StateMachines)
                {
                    stateresult = machine.ProcessState(stateresult, delta);
                }
            }
            catch (StateInterrupt interrupt)
            {
                CascadeThroughStates(interrupt.NextStateType, interrupt.Configs);
            }
        }
    }
}
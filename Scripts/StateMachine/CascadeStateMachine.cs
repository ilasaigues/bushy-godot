using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BushyCore
{
    public abstract partial class CascadeStateMachine<TAgent> : Node where TAgent : Node
    {
        TAgent Agent;
        public List<StateMachine<TAgent>> StateMachines;
        public override void _Ready()
        {
            StateMachines = GetChildren().OfType<StateMachine<TAgent>>().ToList();
        }

        public void OnArea2DEnter(Area2D areaNode)
        {
            TryToChain(sm => sm.OnArea2DInteraction(areaNode, true));
        }

        public void OnArea2DExit(Area2D areaNode)
        {
            TryToChain(sm => sm.OnArea2DInteraction(areaNode, false));
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



        public void SetState(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            var interrupt = new StateInterrupt(stateType, configs: configs);
            CascadeThroughStates(interrupt.NextStateType, interrupt.Configs);
        }

        private void CascadeThroughStates(Type stateType, params StateConfig.IBaseStateConfig[] configs)
        {
            bool stateSet = false;
            foreach (var machine in StateMachines)
            {
                if (machine.SetState(stateType, configs))
                {
                    stateSet |= true;
                }
            }
            if (!stateSet)
                throw new Exception($"Type {stateType} is not a state in any player state machine!");
        }
        public override void _PhysicsProcess(double delta)
        {
            try
            {
                var stateresult = default(StateExecutionStatus);
                foreach (var machine in StateMachines)
                {
                    if (stateresult.StateExecutionResult == StateExecutionResult.Continue)
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
using System;
using System.Linq;
using Godot;

namespace BushyCore
{
    public class StateInterrupt : Exception
    {
        public readonly Type NextStateType;
        public bool StopStateMachine { get; }

        public readonly StateConfig.IBaseStateConfig[] Configs;
        private readonly string message;
        public StateInterrupt(Type nextStateType, bool stopStateMachine = false, params StateConfig.IBaseStateConfig[] configs)
        {
            NextStateType = nextStateType;
            StopStateMachine = stopStateMachine;

            message = $"Changed to state {nextStateType}";
            Configs = configs;
        }

        public static StateInterrupt New<T>(bool stopStateMachine = false, params StateConfig.IBaseStateConfig[] configs) where T : IState
        {
            return new StateInterrupt(typeof(T), stopStateMachine, configs);
        }

        public override string Message
        {
            get { return message; }
        }


    }
}
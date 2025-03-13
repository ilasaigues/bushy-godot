using System;

namespace BushyCore
{
    public class StateInterrupt : Exception
    {
        public readonly Type NextStateType;

        public readonly StateConfig.IBaseStateConfig[] Configs;
        private readonly string message;
        public StateInterrupt(Type nextStateType, params StateConfig.IBaseStateConfig[] configs)
        {
            Configs = configs;
            NextStateType = nextStateType;
            message = $"Changed to state {nextStateType}";
        }

        public override string StackTrace
        {
            get { return message; }
        }
    }

    public class StateInterrupt<TNextState> : StateInterrupt where TNextState : IState
    {
        public StateInterrupt(params StateConfig.IBaseStateConfig[] configs)
            : base(typeof(TNextState), configs) { }
    }
}
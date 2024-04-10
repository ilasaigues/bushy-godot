using System;

namespace BushyCore
{
    public class StateEndedException : Exception
    {
        private string message;
        public StateEndedException(BaseState baseState)
        {
            message = $"State Ended Ended via exception signal. {baseState}";
        }
        public override string StackTrace 
        {
            get { return message; }
        }
    }   
}
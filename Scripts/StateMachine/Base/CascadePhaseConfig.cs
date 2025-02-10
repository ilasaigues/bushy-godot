namespace BushyCore
{
    public class CascadePhaseConfig 
    {
        public bool IsCommitedAction { get; set; } = false;
        public bool X_AxisControlEnabled { get; set; } = true;
        public AnimationLevel CurrentAnimationLevel { get; set; } = AnimationLevel.REGULAR;
        public bool MovementPhaseDisabled { get; set; } = false;

        public enum AnimationLevel 
        {
            UNINTERRUPTIBLE,
            REGULAR,    
        }

        public CascadePhaseConfig(
            bool commitedAction = false,
            bool xAxisEnabled = true, 
            AnimationLevel animationLevel = AnimationLevel.REGULAR, 
            bool movDisabled = false
        )
        {
            IsCommitedAction = commitedAction;
            X_AxisControlEnabled = xAxisEnabled;
            CurrentAnimationLevel = animationLevel;
            MovementPhaseDisabled = movDisabled;
        }

    }
}
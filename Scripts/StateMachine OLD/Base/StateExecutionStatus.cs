using System;

namespace BushyCore
{

    [Flags]
    public enum StateAnimationLevel
    {
        Regular = 0,
        Uninterruptible = 1,
    }

    [Flags]
    public enum StateExecutionResult
    {
        Continue = 0,
        Block = 1
    }

    [Flags]
    public enum MovementLockFlags
    {
        None = 0,
        HorizontalLock = 1,
        VerticalLock = 2,
    }

    public struct StateExecutionStatus
    {
        public StateExecutionResult StateExecutionResult = StateExecutionResult.Continue;
        public MovementLockFlags MovementLockFlags = MovementLockFlags.None;
        public StateAnimationLevel AnimationLevel = StateAnimationLevel.Regular;

        public bool CanMoveHorizontal => !MovementLockFlags.HasFlag(MovementLockFlags.HorizontalLock);
        public bool CanMoveVertical => !MovementLockFlags.HasFlag(MovementLockFlags.VerticalLock);
        public bool CanChangeAnimation => !AnimationLevel.HasFlag(StateAnimationLevel.Uninterruptible);

        public StateExecutionStatus(
            StateExecutionResult executionResult,
            MovementLockFlags movementLockFlags,
            StateAnimationLevel animationLevel)
        {
            StateExecutionResult = executionResult;
            MovementLockFlags = movementLockFlags;
            AnimationLevel = animationLevel;
        }

        public StateExecutionStatus(
            StateExecutionStatus previous,
            StateExecutionResult executionResult = StateExecutionResult.Continue,
            MovementLockFlags movementLockFlags = MovementLockFlags.None,
            StateAnimationLevel animationLevel = StateAnimationLevel.Regular)
        {
            StateExecutionResult = previous.StateExecutionResult | executionResult;
            MovementLockFlags = previous.MovementLockFlags | movementLockFlags;
            AnimationLevel = previous.AnimationLevel | animationLevel;
        }
    }
}
using Godot;
using System;

namespace BushyCore
{
    public partial class StateConfig : GodotObject
    {
        public interface IBaseStateConfig { }
        public struct InitialVerticalVelocityConfig : IBaseStateConfig
        {
            public float Velocity;
            public InitialVerticalVelocityConfig(float velocity) { Velocity = velocity; }
        }

        public struct InitialVelocityVectorConfig : IBaseStateConfig
        {
            public Vector2 Velocity;
            public bool DoesDecelerate;
            public bool CanEnterHedge;
            public InitialVelocityVectorConfig(Vector2 velocity, bool doesDecelerate = true, bool canEnterHedge = false)
            {
                Velocity = velocity;
                DoesDecelerate = doesDecelerate;
                CanEnterHedge = canEnterHedge;
            }
        }

        public static InitialVerticalVelocityConfig InitialVerticalVelocity(float velocity)
        {
            return new InitialVerticalVelocityConfig(velocity);
        }
        public static InitialVelocityVectorConfig InitialVelocityVector(Vector2 velocity, bool doesDecelerate = true, bool canEnterHedge = false)
        {
            return new InitialVelocityVectorConfig(velocity, doesDecelerate, canEnterHedge);
        }

        public struct InitialSwingHookConfig : IBaseStateConfig
        {
            public Vector2 HitPoint;
            public bool EarlyRelease;
            public InitialSwingHookConfig(Vector2 hitPoint, bool earlyRelease) { HitPoint = hitPoint; EarlyRelease = earlyRelease; }
        }
        public static InitialSwingHookConfig InitialSwingHookCollision(Vector2 hitPoint, bool earlyRelease)
        {
            return new InitialSwingHookConfig(hitPoint, earlyRelease);
        }

        public struct InitialHedgeConfig : IBaseStateConfig
        {
            public HedgeNode Hedge;
            public Vector2 Direction;
            public InitialHedgeConfig(HedgeNode hedge, Vector2 dir)
            {
                Hedge = hedge;
                Direction = dir;
            }
        }

        public static InitialHedgeConfig InitialHedgeCollider(HedgeNode hedge, Vector2 dir)
        {
            return new InitialHedgeConfig(hedge, dir);
        }

        public struct InitialGroundedConfig : IBaseStateConfig
        {
            public bool IsJumpBuffered;
            public bool DoesDecelerate;
            public InitialGroundedConfig(bool canBufferJump = false, bool doesDecelerate = true)
            {
                IsJumpBuffered = canBufferJump;
                DoesDecelerate = doesDecelerate;
            }
        }
        public static InitialGroundedConfig InitialGroundedJumpBuffer(bool canBufferJump)
        {
            return new InitialGroundedConfig(canBufferJump);
        }
        public static InitialGroundedConfig InitialGrounded(bool doesDecelerate)
        {
            return new InitialGroundedConfig(doesDecelerate: doesDecelerate);
        }

        public struct InitialAnimationConfig : IBaseStateConfig
        {
            public string AnimationName;
            public InitialAnimationConfig(string animationName)
            {
                AnimationName = animationName;
            }
        }

    }
}

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
            public InitialVelocityVectorConfig(Vector2 velocity, bool doesDecelerate = true) 
            { 
                Velocity = velocity; 
                DoesDecelerate = doesDecelerate; 
            }
        }

        public static InitialVerticalVelocityConfig InitialVerticalVelocity(float velocity)
        {
            return new InitialVerticalVelocityConfig(velocity);
        }
        public static InitialVelocityVectorConfig InitialVelocityVector(Vector2 velocity, bool doesDecelerate = true)
        {
            return new InitialVelocityVectorConfig(velocity, doesDecelerate);
        }

        public struct StartJumpConfig : IBaseStateConfig { }
        public static StartJumpConfig StartJump()
        {
            return new StartJumpConfig();
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

        public struct InitialGroundedConfig : IBaseStateConfig
        {
            public bool CanBufferJump;
            public bool DoesDecelerate;
            public InitialGroundedConfig(bool canBufferJump = true, bool doesDecelerate = true) 
            {
                CanBufferJump = canBufferJump; 
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

    }
}

using System.Diagnostics;
using Godot;
using static MovementComponent;

namespace BushyCore 
{
    public partial class AirState : BaseState
    {
        private double verticalVelocity;
        private double horizontalVelocity;


        public override void StateEnter(params StateConfig.IBaseStateConfig[] configs)
        {
            base.StateEnter(configs);
            // _directionInputSubscription = _input.DirectionInput.Subscribe(UpdateFacingDirection);
            horizontalVelocity = movementComponent.Velocities[VelocityType.MainMovement].X;
            verticalVelocity = 0;
            SetupFromConfigs(configs);
        }
        
        private void SetupFromConfigs(StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.StartJumpConfig)
                {
                    verticalVelocity = characterVariables.JumpSpeed;
                    actionsComponent.CanJump = false;
                }
                if (config is StateConfig.InitialVelocityVectorConfig velocityConfig)
                {
                    verticalVelocity = velocityConfig.Velocity.Y;
                }
            }
        }

        public override void StateExit()
        {
            base.StateExit();
            // _directionInputSubscription?.Dispose();
        }

        public override void StateUpdateInternal(double delta)
        {
            // if (TryCoyoteJump()) return;
            HandleGravity(delta);
            HandleHorizontalMovement(delta);
            CheckTransitions();
            // CheckSwing();
            
            movementComponent.Velocities[VelocityType.Gravity] = new Vector2(0, (float) verticalVelocity);
            movementComponent.Velocities[VelocityType.MainMovement] = (float) horizontalVelocity * Vector2.Right;
        }

        // void UpdateFacingDirection(Vector2 dir)
        // {
        //     _charController.FacingDirection = dir.x != 0 ?
        //         new Vector2(dir.x, 0) :
        //         _charController.FacingDirection;
        // }
        void HandleGravity(double deltaTime)
        {
            verticalVelocity = Mathf.Min(characterVariables.AirTerminalVelocity, verticalVelocity + GetGravity() * (float) deltaTime);
        }
        // bool TryCoyoteJump()
        // {
        //     if (actionsComponent.CanJump && TimeSinceStateStart <= _charVariables.CoyoteJumpTime && _charController.JumpPressed)
        //     {
        //         _charController.ChangeState<JumpState>();
        //         return   true;
        //     }
        //     return false;
        // }

        float GetGravity()
        {
            if (verticalVelocity <= characterVariables.AirSpeedThresholds.Y)
            {
                return characterVariables.AirGravity;
            }
            else if (verticalVelocity <= characterVariables.AirSpeedThresholds.X)
            {
                return characterVariables.AirApexGravity;
            }
            else
            {
                return characterVariables.AirGravity;
            }
        }

        void HandleHorizontalMovement(double deltaTime)
        {
            Vector2 direction = actionsComponent.MovementDirection;
            if (direction.X != 0) // If we're doing any input
            {

                //if the input direction is opposite of the current direction, we also add a deceleration
                if (direction.X * horizontalVelocity < 0)
                {
                    horizontalVelocity += direction.X * characterVariables.AirHorizontalDeceleration * deltaTime;
                }

                if (Mathf.Abs(horizontalVelocity) <= Mathf.Abs(characterVariables.AirHorizontalMovementSpeed))
                {
                    horizontalVelocity += direction.X * characterVariables.AirHorizontalAcceleration * deltaTime;
                    horizontalVelocity = Mathf.Clamp(horizontalVelocity, -characterVariables.AirHorizontalMovementSpeed, characterVariables.AirHorizontalMovementSpeed);
                }
                else
                {
                    horizontalVelocity += characterVariables.AirHorizontalOvercappedDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                }

            }
            else //if we're not doing any input, we decelerate to 0
            {
                var deceleration = characterVariables.AirHorizontalDeceleration * deltaTime * (horizontalVelocity > 0 ? -1 : 1);
                if (Mathf.Abs(deceleration) < Mathf.Abs(horizontalVelocity))
                {
                    horizontalVelocity += deceleration;
                }
                else
                {
                    horizontalVelocity = 0;
                }
            }
        }

        void CheckTransitions() 
        {
            if (actionsComponent.IsDashRequested && actionsComponent.CanDash)
                actionsComponent.Dash(this.IntendedDirection);

            if (!movementComponent.IsOnFloor) return;
            
            if (verticalVelocity > 0) 
            {
                if (actionsComponent.IsJumpRequested) 
                    actionsComponent.Jump();
                actionsComponent.Land();
            }
        }
        public override void OnRigidBodyEnter(Node node) 
        {
            // foreach (var contact in collision.contacts)
            // {
            //     var isPlatform = contact.collider.GetComponent<PlatformEffector2D>();
            //     //if contact normal is up and contact is under me
            //     var dot = Vector2.Dot(contact.normal, Vector2.up);
            //     if (dot > 0.1f && contact.point.y < _charController.transform.position.y)
            //     {
            //         if (_verticalVelocity < 0) //if I'm going down,
            //             _charController.ChangeState<GroundedState>();
            //     }

            //     //if contact normal is down (exception added in case we're colliding with a one-way platform)
            //     if (dot < -0.5f)
            //     {
            //         if (isPlatform) continue;
            //         _verticalVelocity = 0;
            //     }
            //     //if contact normal is not vertical and is opposite of movement (exception added in case we're colliding with a one-way platform)
            //     if (Mathf.Abs(dot) < 0.5 && contact.normal.x * _horizontalVelocity < 0)
            //     {
            //         if (isPlatform) continue;
            //         _horizontalVelocity = 0;
            //     }
            // }
            // _charController.Velocities[CharacterController.VelocityType.Gravity] = new Vector2(0, _verticalVelocity);
            // _charController.Velocities[CharacterController.VelocityType.MainMovement] = _horizontalVelocity * Vector2.right;
        }
    }
}
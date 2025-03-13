using System;
using System.Diagnostics;
using Godot;

namespace BushyCore
{
    public class AxisMovement
    {
        private int Acceleration;
        private int Deceleration;
        private int MovementSpeed;
        private float OvercappedDeceleration;
        private int TurnDeceleration;
        private int? ThresholdSpeed;
        private MovementComponent Movement;
        private CharacterVariables Variables;
        private Func<float> Direction;
        private Func<int, bool> CollisionCheck;
        private int _axisDirection = 1;

        public bool HasOvershootDeceleration { get; private set; }
        public double Velocity { get; private set; }
        private AxisMovement() { }
        private AxisMovement(
            int _acceleration,
            int _deceleration,
            int _movementSpeed,
            int _turnDeceleration,
            float _overcappedDeceleration,
            int? _thresholdSpeed,
            MovementComponent _movement,
            Func<float> _direction,
            CharacterVariables _variables,
            Func<int, bool> _collisionCheck)
        {
            this.Acceleration = _acceleration;
            this.Deceleration = _deceleration;
            this.MovementSpeed = _movementSpeed;
            this.OvercappedDeceleration = _overcappedDeceleration;
            this.TurnDeceleration = _turnDeceleration;
            this.MovementSpeed = _movementSpeed;
            this.OvercappedDeceleration = _overcappedDeceleration;
            this.ThresholdSpeed = _thresholdSpeed;
            this.Movement = _movement;
            this.Direction = _direction;
            this.Variables = _variables;
            this.CollisionCheck = _collisionCheck;

            this.HasOvershootDeceleration = true;
        }

        public void OvershootDec(bool val) { this.HasOvershootDeceleration = val; }
        public void SetInitVel(double val) { this.Velocity = val; }

        public void HandleMovement(double deltaTime)
        {
            float currDirection = Direction.Invoke();
            var vars = Variables;

            _axisDirection = Velocity == 0f ? _axisDirection : Mathf.Sign(Velocity);

            if (currDirection != 0)
            {
                var isColliding = CollisionCheck.Invoke(_axisDirection);
                var targetVelocity = isColliding ? vars.MaxOnWallHorizontalMovementSpeed : MovementSpeed;
                //if the input direction is opposite of the current direction, we also add a deceleration
                if (currDirection * Velocity < 0)
                {
                    Velocity += currDirection * TurnDeceleration * deltaTime;
                }
                else if (Mathf.Abs(Velocity) <= Mathf.Abs(targetVelocity))
                {
                    Velocity += currDirection * Acceleration * deltaTime;
                    Velocity = Mathf.Clamp(Velocity, -targetVelocity, targetVelocity);
                }
                else if (HasOvershootDeceleration || isColliding)
                {
                    Velocity += OvercappedDeceleration * deltaTime * (Velocity > 0 ? -1 : 1);
                    Velocity = Mathf.Max(targetVelocity, Mathf.Abs(Velocity)) * Mathf.Sign(_axisDirection);
                }
            }
            else //if we're not doing any input, we decelerate to 0
            {
                if (Velocity == 0)
                    return;

                double deceleration = Deceleration * deltaTime * (Velocity > 0 ? -1 : 1); ;

                if ((HasOvershootDeceleration && Mathf.Abs(Velocity) > MovementSpeed) || Movement.IsOnWall)
                {
                    deceleration = OvercappedDeceleration * deltaTime * (Velocity > 0 ? -1 : 1);
                }

                if (Mathf.Abs(deceleration) < Mathf.Abs(Velocity))
                {
                    Velocity += deceleration;
                }
                else
                {
                    Velocity = 0;
                }
            }

            if (ThresholdSpeed.HasValue && Mathf.Abs(Velocity) > ThresholdSpeed.Value)
                Velocity = _axisDirection * ThresholdSpeed.Value;
        }
        public Builder ToBuilder()
        {
            return new Builder().Acc(this.Acceleration)
                .Dec(this.Deceleration)
                .Speed(this.MovementSpeed)
                .OverDec(this.OvercappedDeceleration)
                .TurnDec(this.Acceleration)
                .OverDec(this.OvercappedDeceleration)
                .ThresSpeed(this.ThresholdSpeed)
                .Movement(this.Movement)
                .Direction(this.Direction)
                .Variables(this.Variables)
                .Vel(this.Velocity)
                .HasOvershoot(this.HasOvershootDeceleration)
                .AxisDir(this._axisDirection)
                .ColCheck(this.CollisionCheck);
        }
        public class Builder
        {
            public Builder() { }

            private int? _acceleration;
            private int? _deceleration;
            private int? _movementSpeed;
            private float _overcappedDeceleration = 0;
            private int? _turnDeceleration;
            private int? _thresholdSpeed;
            private MovementComponent _movement;
            private Func<float> _direction;
            private CharacterVariables _variables;
            private Func<int, bool> _collisionCheck;

            // Optional state variables
            private int _axisDirection = 1;
            private bool _hasOvershootVel;
            private double _velocity;

            public Builder Acc(int? val) { _acceleration = val; return this; }
            public Builder Dec(int? val) { _deceleration = val; return this; }
            public Builder Speed(int? val) { _movementSpeed = val; return this; }
            public Builder OverDec(float val) { _overcappedDeceleration = val; return this; }
            public Builder TurnDec(int? val) { _turnDeceleration = val; return this; }
            public Builder ThresSpeed(int? val) { _thresholdSpeed = val; return this; }
            public Builder Movement(MovementComponent movement) { _movement = movement; return this; }
            public Builder Direction(Func<float> direction) { _direction = direction; return this; }
            public Builder Variables(CharacterVariables variables) { _variables = variables; return this; }

            public Builder AxisDir(int axisDirection) { _axisDirection = axisDirection; return this; }
            public Builder HasOvershoot(bool hasOvershootVel) { _hasOvershootVel = hasOvershootVel; return this; }
            public Builder Vel(double velocity) { _velocity = velocity; return this; }
            public Builder ColCheck(Func<int, bool> collisionCheck) { _collisionCheck = collisionCheck; return this; }
            public AxisMovement Build()
            {
                if (_acceleration == null) throw new System.Exception("No horizontal acceleration set");
                if (_deceleration == null) throw new System.Exception("No horizontal decelartion set");
                if (_movementSpeed == null) throw new System.Exception("No horizontal mov speed set");
                if (_turnDeceleration == null) throw new System.Exception("No turn deceleration set");
                if (_movement == null) throw new System.Exception("No movement set");
                if (_direction == null) throw new System.Exception("No actions set");
                if (_variables == null) throw new System.Exception("No char variables set");
                if (_collisionCheck == null) throw new System.Exception("No collision checker variables set");

                AxisMovement axisMovement = new AxisMovement(
                    _acceleration.Value,
                    _deceleration.Value,
                    _movementSpeed.Value,
                    _turnDeceleration.Value,
                    _overcappedDeceleration,
                    _thresholdSpeed,
                    _movement,
                    _direction,
                    _variables,
                    _collisionCheck
                );

                axisMovement.SetInitVel(_velocity);
                axisMovement._axisDirection = _axisDirection;
                axisMovement.OvershootDec(_hasOvershootVel);

                return axisMovement;
            }

            public Builder Copy()
            {
                return new Builder()
                    .Acc(_acceleration)
                    .Dec(_deceleration)
                    .Speed(_movementSpeed)
                    .OverDec(_overcappedDeceleration)
                    .TurnDec(_turnDeceleration)
                    .ThresSpeed(_thresholdSpeed)
                    .Movement(_movement)
                    .Direction(_direction)
                    .Variables(_variables)
                    .Vel(_velocity)
                    .HasOvershoot(_hasOvershootVel)
                    .AxisDir(_axisDirection)
                    .ColCheck(_collisionCheck);
            }

        }
    }

}
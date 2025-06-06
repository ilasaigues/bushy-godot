using Godot;

namespace BushyCore
{
    public partial class ProjectileAttackState : BaseState<PlayerController>
    {

        [Export] PackedScene _attackProjectileScene;

        private bool animationOver;

        private Vector2 _spawnPosition;

        private Vector2 _spawnDirection;
        private bool _flipHorizontal;

        private const string ANIMATION_FINISHED_MESSAGE = "ProjectileAnimationEnded";

        bool _triggerSpawn;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            animationOver = false;
            Agent.PlayerInfo.IsAttacking = true;
            SetUpFromConfigs(configs);
        }

        private void SetUpFromConfigs(params StateConfig.IBaseStateConfig[] configs)
        {
            foreach (var config in configs)
            {
                if (config is StateConfig.FireProjectileConfig projectileConfig)
                {
                    _spawnDirection = projectileConfig.Direction;
                    _flipHorizontal = projectileConfig.FlipHorizontal;
                    Agent.AnimController.SetTrigger(PlayerController.AnimConditions.ProjectileAttackTrigger);
                    Agent.AnimController.SetBlendValue(PlayerController.AnimConditions.ProjectileDirectionBlendValues, _spawnDirection.Normalized());
                    Agent.AnimController.AnimationMessageSent += OnAnimationMessage;
                    Agent.PlayerInfo.CanFallIntoHedge = false;
                    Agent.CollisionComponent.ToggleHedgeCollision(true);
                }
            }
        }

        private void OnAnimationMessage(string message)
        {
            if (message == ANIMATION_FINISHED_MESSAGE)
            {
                animationOver = true;
            }
        }

        protected override void ExitStateInternal()
        {
            if (_triggerSpawn)
            {
                SpawnProjectile();
            }
            Agent.AnimController.AnimationMessageSent -= OnAnimationMessage;
            _triggerSpawn = false;
            Agent.PlayerInfo.IsAttacking = false;
        }

        public void SetProjectileSpawnPoint(Vector2 projectileSpawnPosition)
        {
            if (!Active)
            {
                return;
            }
            _spawnPosition = projectileSpawnPosition;
            _triggerSpawn = true;

            GD.Print("SETTING SPAWN TRIGGER");
        }

        private void SpawnProjectile()
        {
            GD.Print("SPAWNING");

            var projectileNode = (BaseProjectile)_attackProjectileScene.Instantiate();
            var correction = Vector2.Zero;
            if (_flipHorizontal)
            {
                correction = (Agent.GlobalPosition - _spawnPosition) * 2;
                correction.Y = 0;
            }
            if (Mathf.Abs(_spawnDirection.Dot(Vector2.Right)) > 0.866025403784) // sin(60) = 0.866025403784
            {
                _spawnDirection.Y = 0;
            }
            else
            {
                _spawnDirection.X = 0;
            }

            _spawnDirection = _spawnDirection.Normalized();
            GD.Print(_spawnDirection);
            projectileNode.GlobalRotation = _spawnDirection.Angle();
            projectileNode.GlobalPosition = _spawnPosition + correction;
            Agent.AddSibling(projectileNode);
            projectileNode.Fire(
                Agent.CharacterVariables.ProjectileLifetime,
                _spawnDirection * Agent.CharacterVariables.ProjectileSpeed,
                Vector2.Zero);
            _triggerSpawn = false;
        }

        private void TransitionOut()
        {
            ChangeState<INullState>(true);
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (_triggerSpawn)
            {
                SpawnProjectile();
            }

            if (animationOver)
            {
                TransitionOut();
            }

            prevStatus.AnimationLevel = StateAnimationLevel.Uninterruptible;
            if (Agent.MovementComponent.IsOnFloor)
            {
                prevStatus.MovementLockFlags |= MovementLockFlags.HorizontalLock;
            }
            return prevStatus;
        }
    }
}
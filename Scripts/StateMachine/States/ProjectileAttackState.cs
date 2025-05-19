using Godot;

namespace BushyCore
{
    public partial class ProjectileAttackState : BaseState<PlayerController>
    {

        [Export] PackedScene _attackProjectileScene;

        private double _timeLeft = 0.4167;

        private Vector2 _spawnPosition;

        private Vector2 _spawnDirection;
        private bool _flipHorizontal;

        bool _triggerSpawn;

        protected override void EnterStateInternal(params StateConfig.IBaseStateConfig[] configs)
        {
            _timeLeft = 0.4167;
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
                }
            }
        }

        protected override void ExitStateInternal()
        {
            if (_triggerSpawn)
            {
                SpawnProjectile();
            }
            _triggerSpawn = false;
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
            projectileNode.GlobalPosition = _spawnPosition + correction;
            projectileNode.GlobalRotation = _spawnDirection.Angle();
            Agent.AddSibling(projectileNode);
            projectileNode.Fire(
                Agent.CharacterVariables.ProjectileLifetime,
                _spawnDirection * Agent.CharacterVariables.ProjectileSpeed,
                Vector2.Zero);
            _triggerSpawn = false;
        }

        protected override StateExecutionStatus ProcessStateInternal(StateExecutionStatus prevStatus, double delta)
        {
            if (_triggerSpawn)
            {
                SpawnProjectile();
            }

            if (_timeLeft <= 0)
            {
                if (Agent.MovementComponent.IsOnFloor)
                {
                    throw StateInterrupt.New<IdleGroundedState>(true);
                }
                else
                {
                    throw StateInterrupt.New<FallState>(true);
                }
            }
            _timeLeft -= delta;





            prevStatus.AnimationLevel = StateAnimationLevel.Uninterruptible;
            return prevStatus;
        }
    }
}
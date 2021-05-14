namespace NuggetBlaster.Entities
{
    class EnemyEntity : Entity
    {
        public override int BaseSpeed { get; set; } = 600;
        public override bool MoveLeft { get; set; } = true;
        public override int Team { get; set; } = 2;
        public override bool CanShoot { get; set; } = true;
        public override long ShootCooldownTimer { get; set; } = 0;
        public override int ShootCooldownMS { get; set; } = 1500;

        public EnemyEntity()
        {
            MaxSpeed = BaseSpeed;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            return new ProjectileEntity
            {
                MoveLeft = true,
                MaxSpeed = (int) (BaseSpeed * 1.5),
                Team = Team
            };
        }
    }
}
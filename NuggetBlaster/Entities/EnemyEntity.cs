using NuggetBlaster.Properties;
using System.Drawing;

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
        public override int PointsOnKill { get; set; } = 100;

        public EnemyEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite) { }

        public override ProjectileEntity Shoot(Rectangle spriteRectangle, Image sprite)
        {
            ShootCooldown();
            return new ProjectileEntity(spriteRectangle, sprite)
            {
                MoveLeft = true,
                MaxSpeed = (int) (BaseSpeed * 1.5),
                Team = Team
            };
        }
    }
}
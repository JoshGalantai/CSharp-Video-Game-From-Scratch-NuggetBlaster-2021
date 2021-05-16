using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class EnemyEntity : Entity
    {
        public EnemyEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite)
        {
            MoveLeft           = true;
            Team               = 2;
            PointsOnKill       = 100;
            ShootCooldownMS    = 1500;
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS/5;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            Point location = new(SpriteRectangle.Left - 20, SpriteRectangle.Top + (SpriteRectangle.Height / 2));
            return new ProjectileEntity(new Rectangle(location, new Size(30, 16)), Resources.enemyProjectile)
            {
                MoveLeft = true,
                MaxSpeed = (int) (MaxSpeed * 1.2),
                Team     = Team
            };
        }
    }
}
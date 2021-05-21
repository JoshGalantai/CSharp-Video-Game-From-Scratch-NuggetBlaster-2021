using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class EnemyEntity : Entity
    {
        public EnemyEntity(Rectangle gameCanvas, Rectangle spriteRectangle, Image sprite = null) : base(gameCanvas, spriteRectangle, sprite)
        {
            MoveLeft           = true;
            Team               = 2;
            PointsOnKill       = 100;
            ShootCooldownMS    = 2500;
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS/5;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            Point location = new(SpriteRectangle.Left - 20, SpriteRectangle.Top + (SpriteRectangle.Height/2) - (ProjectileHeight/2));
            return new ProjectileEntity(GameRectangle, new Rectangle(location, new Size(ProjectileWidth, ProjectileHeight)), Resources.enemyProjectile)
            {
                MoveLeft   = true,
                BaseSpeed  = BaseSpeed,
                SpeedMulti = 1.4,
                Team       = Team
            };
        }
    }
}
using NuggetBlaster.Properties;
using System;
using System.Collections.Generic;
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
            ShootCooldownMS    = 2000;
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS / 10; // Delay before first shot after spawn
            Damage             = 1;
        }

        public override List<ProjectileEntity> Shoot()
        {
            List<ProjectileEntity> projList = new();
            if (CheckCanShoot())
            {
                ShootCooldown();
                Point location = new(SpriteRectangle.Left - 20, SpriteRectangle.Top + (SpriteRectangle.Height / 2) - (ProjectileHeight / 2));
                projList.Add(new ProjectileEntity(GameRectangle, new Rectangle(location, new Size(ProjectileWidth, ProjectileHeight)), Resources.enemyProjectile)
                {
                    MoveLeft = true,
                    BaseSpeed = BaseSpeed,
                    SpeedMulti = 1.4,
                    Team = Team,
                    Damage = Damage
                });
            }
            return projList;
        }
    }
}
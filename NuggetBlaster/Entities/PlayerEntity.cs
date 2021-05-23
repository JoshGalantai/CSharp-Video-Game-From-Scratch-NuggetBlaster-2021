using NuggetBlaster.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public const int MaxShootBuffLevel = 3;

        public long DamageableCooldownTimer { get; set; } = 0;
        public int  DamageableCooldownMS    { get; set; } = 500;
        public int  ShootBuffLevel          { get; set; } = 0;

        public PlayerEntity(Rectangle gameCanvas) : base(gameCanvas, new Rectangle(0, gameCanvas.Height/2, (int)(gameCanvas.Width*0.1), (int)(gameCanvas.Width*0.05)), Resources.nugget) {
            BaseSpeed       = 0.2;
            Team            = 1;
            CanShoot        = true;
            Damage          = 1;
            ShootCooldownMS = 400;
        }

        public override void TakeDamage(int Damage)
        {
            if (! Damageable || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < DamageableCooldownTimer)
                return;

            HitPoints -= Damage;
            ShootBuffLevel = 0;
            DamageableCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + DamageableCooldownMS;
        }

        public override bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer && Spacebar;
        }

        public override List<ProjectileEntity> Shoot()
        {
            List<ProjectileEntity> projList = new();
            if (CheckCanShoot())
            {
                ShootCooldown();
                int x = SpriteRectangle.Right + 20;
                int y = SpriteRectangle.Top + (SpriteRectangle.Height / 2) - (ProjectileHeight / 2);

                ProjectileEntity projectile = new(GameRectangle, new Rectangle(new Point(x, y + (ShootBuffLevel == 1 ? (ProjectileHeight / 2) : 0)), new Size(ProjectileWidth, ProjectileHeight)), Resources.allyProjectile)
                {
                    MoveRight  = true,
                    BaseSpeed  = BaseSpeed,
                    SpeedMulti = 2.0,
                    Team       = Team,
                    Damage     = Damage
                };
                projList.Add(projectile);

                if (ShootBuffLevel >= 1)
                {
                    ProjectileEntity projectileTwo = (ProjectileEntity)projectile.Clone();
                    projectileTwo.SpriteRectangle  = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y - ProjectileHeight), projectile.SpriteRectangle.Size);
                    projList.Add(projectileTwo);
                }
                if (ShootBuffLevel >= 2)
                {
                    ProjectileEntity projectileThree = (ProjectileEntity)projectile.Clone();
                    projectileThree.SpriteRectangle = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y + ProjectileHeight), projectile.SpriteRectangle.Size);
                    projList.Add(projectileThree);
                }
                if (ShootBuffLevel >= 3)
                {
                    ProjectileEntity projectileFour = (ProjectileEntity)projectile.Clone();
                    projectileFour.MoveUp = true;
                    projList.Add(projectileFour);
                    ProjectileEntity projectileFive = (ProjectileEntity)projectile.Clone();
                    projectileFive.MoveDown = true;
                    projList.Add(projectileFive);
                }
            }
            return projList;
        }

        // Make sure player does not go out of bounds
        public override void ProcessMovement(Rectangle proposedRectangle)
        {
            int x = proposedRectangle.X < GameRectangle.X ? GameRectangle.X : proposedRectangle.X;
            int y = proposedRectangle.Y < GameRectangle.Y ? GameRectangle.Y : proposedRectangle.Y;
            x = proposedRectangle.X > GameRectangle.Width - proposedRectangle.Width ? GameRectangle.Width - proposedRectangle.Width : x;
            y = proposedRectangle.Y > GameRectangle.Height - proposedRectangle.Height ? GameRectangle.Height - proposedRectangle.Height : y;

            SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
        }
    }
}
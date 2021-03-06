using NuggetBlaster.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public const int  MaxShootBuffLevel                     = 4; // Highest buff level allowed
        public       int  ShootBuffLevel          { get; set; } = 0; // Each buff level enhances players shooting
        public       long DamageableCooldownTimer { get; set; } = 0;
        private      int  DamageableCooldownMS    { get; set; } = 1000; // After getting hit player is invulnerable for a brief period


        public PlayerEntity(Rectangle gameCanvas) : base(gameCanvas, new Rectangle(0, gameCanvas.Height/2, (int)(gameCanvas.Width*0.1), (int)(gameCanvas.Width*0.05)), Resources.nugget) {
            BaseSpeed       = 0.2;
            Team            = 1;
            CanShoot        = true;
            ShootCooldownMS = 400;
        }

        public override void TakeDamage(Entity entity)
        {
            if (! Damageable || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < DamageableCooldownTimer)
                return;

            HitPoints -= entity.Damage;
            if (ShootBuffLevel > 0)
                ShootBuffLevel--;

            DamageableCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + DamageableCooldownMS;
        }

        protected override bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer && Spacebar;
        }

        public override List<ProjectileEntity> Shoot()
        {
            List<ProjectileEntity> projList = new();
            if (CheckCanShoot())
            {
                ShootCooldown();
                int x      = SpriteRectangle.Right + 20;
                int y      = SpriteRectangle.Top + (SpriteRectangle.Height / 2) - (ProjectileHeight / 2);
                var sprite = ShootBuffLevel >= 4 ? Resources.allyProjectileSuper : Resources.allyProjectile;
                var multi  = ShootBuffLevel >= 4 ? 3.0 : 2.0;

                ProjectileEntity projectile = new(GameRectangle, new Rectangle(new Point(x, y + (ShootBuffLevel == 1 ? (ProjectileHeight / 2) : 0)), new Size(ProjectileWidth, ProjectileHeight)), sprite)
                {
                    MoveRight  = true,
                    BaseSpeed  = BaseSpeed,
                    SpeedMulti = multi,
                    Team       = Team,
                    Damage     = Damage
                };
                projList.Add(projectile);

                if (ShootBuffLevel >= 1)
                {
                    ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
                    proj.SpriteRectangle  = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y - ProjectileHeight), projectile.SpriteRectangle.Size);
                    projList.Add(proj);
                }
                if (ShootBuffLevel >= 2)
                {
                    ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
                    proj.SpriteRectangle = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y + ProjectileHeight), projectile.SpriteRectangle.Size);
                    projList.Add(proj);
                }
                if (ShootBuffLevel >= 3)
                {
                    ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
                    proj.MoveUp = true;
                    projList.Add(proj);
                    proj = (ProjectileEntity)projectile.Clone();
                    proj.MoveDown = true;
                    projList.Add(proj);
                }
            }
            return projList;
        }

        // Make sure player does not go out of bounds
        protected override void ProcessMovement(Rectangle proposedRectangle)
        {
            int x = proposedRectangle.X < GameRectangle.X ? GameRectangle.X : proposedRectangle.X;
            int y = proposedRectangle.Y < GameRectangle.Y ? GameRectangle.Y : proposedRectangle.Y;
            x = proposedRectangle.X > GameRectangle.Width - proposedRectangle.Width ? GameRectangle.Width - proposedRectangle.Width : x;
            y = proposedRectangle.Y > GameRectangle.Height - proposedRectangle.Height ? GameRectangle.Height - proposedRectangle.Height : y;

            SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
        }
    }
}
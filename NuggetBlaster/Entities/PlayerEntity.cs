using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public PlayerEntity(Rectangle gameCanvas) : base(gameCanvas, new Rectangle(0, gameCanvas.Height/2, (int)(gameCanvas.Width*0.1), (int)(gameCanvas.Width*0.05)), Resources.nugget) {
            BaseSpeed       = 0.2;
            Team            = 1;
            CanShoot        = true;
            ShootCooldownMS = 400;
        }

        public override bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer && Spacebar;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            Point location = new(SpriteRectangle.Right + 20, SpriteRectangle.Top + (SpriteRectangle.Height/2) - (ProjectileHeight/2));
            return new ProjectileEntity(GameRectangle, new Rectangle(location, new Size(ProjectileWidth, ProjectileHeight)), Resources.allyProjectile)
            {
                MoveRight  = true,
                BaseSpeed  = BaseSpeed,
                SpeedMulti = 2.0,
                Team       = Team
            };
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
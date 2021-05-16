using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public PlayerEntity() : base(new Rectangle(15, 200, 100, 50), Resources.nugget) {
            BaseSpeed       = 400;
            Team            = 1;
            CanShoot        = true;
            ShootCooldownMS = 200;
        }

        public override bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer && Spacebar;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            Point location = new(SpriteRectangle.Right + 20, SpriteRectangle.Top + (SpriteRectangle.Height / 2));
            return new ProjectileEntity(new Rectangle(location, new Size(30, 16)), Resources.allyProjectile)
            {
                MoveRight = true,
                MaxSpeed  = BaseSpeed * 2,
                Team      = Team
            };
        }

        // Make sure player does not go out of bounds
        public override void ProcessMovement(Rectangle bounds, Rectangle proposedRectangle)
        {
            int x = proposedRectangle.X < bounds.X ? bounds.X : proposedRectangle.X;
            int y = proposedRectangle.Y < bounds.Y ? bounds.Y : proposedRectangle.Y;
            x = proposedRectangle.X > bounds.Width - proposedRectangle.Width ? bounds.Width - proposedRectangle.Width : x;
            y = proposedRectangle.Y > bounds.Height - proposedRectangle.Height ? bounds.Height - proposedRectangle.Height : y;

            SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
        }
    }
}
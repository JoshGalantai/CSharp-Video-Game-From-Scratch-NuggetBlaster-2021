using NuggetBlaster.Properties;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public PlayerEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite) {
            BaseSpeed       = 400;
            Team            = 1;
            CanShoot        = true;
            ShootCooldownMS = 200;
            Sprite          = Resources.Nugget;
            
        }

        public override ProjectileEntity Shoot(Rectangle spriteRectangle, Image sprite)
        {
            ShootCooldown();
            return new ProjectileEntity(spriteRectangle, sprite)
            {
                MoveRight = true,
                MaxSpeed  = BaseSpeed * 2,
                Team      = Team
            };
        }

        // Make sure player does not go out of bounds
        public override void CalculateCollision(Rectangle bounds, Rectangle proposedRectangle)
        {
            int x = proposedRectangle.X < bounds.X ? bounds.X : proposedRectangle.X;
            int y = proposedRectangle.Y < bounds.Y ? bounds.Y : proposedRectangle.Y;
            x = proposedRectangle.X > bounds.Width - proposedRectangle.Width ? bounds.Width - proposedRectangle.Width : x;
            y = proposedRectangle.Y > bounds.Height - proposedRectangle.Height ? bounds.Height - proposedRectangle.Height : y;

            SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
        }
    }
}
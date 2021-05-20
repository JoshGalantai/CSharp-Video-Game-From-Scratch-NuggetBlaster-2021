using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public virtual int BaseSpeed { get; set; } = 200;
        public virtual int MaxSpeed { get; set; } = 0;
        public virtual bool MoveRight { get; set; } = false;
        public virtual bool MoveLeft { get; set; } = false;
        public virtual bool MoveUp { get; set; } = false;
        public virtual bool MoveDown { get; set; } = false;
        public virtual bool Spacebar { get; set; } = false;
        public virtual int Team { get; set; } = 0;
        public virtual bool CanShoot { get; set; } = false;
        public virtual bool Damageable { get; set; } = true;
        public virtual int HitPoints { get; set; } = 1;
        public virtual long ShootCooldownTimer { get; set; } = 0;
        public virtual int ShootCooldownMS { get; set; } = 1500;
        public virtual Image SpriteOriginal { get; set; } = Resources.pickle;
        public virtual Image SpriteResized { get; set; } = Resources.pickle;
        public virtual Rectangle SpriteRectangle { get; set; } = new Rectangle(0, 0, 0, 0);
        public virtual Rectangle CanvasRectangle { get; set; } = new Rectangle(0, 0, 0, 0);
        public virtual int PointsOnKill { get; set; } = 0;
        

        public Entity(/*Rectangle CanvasRectangle, */Rectangle spriteRectangle, Image sprite = null)
        {
            MaxSpeed        = BaseSpeed;
            SpriteRectangle = spriteRectangle;
            if (sprite != null)
                SpriteOriginal = sprite;
            SpriteResized = GameForm.ResizeImage(SpriteOriginal, SpriteRectangle);
        }

        public abstract ProjectileEntity Shoot();

        public void ShootCooldown()
        {
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS;
        }

        public virtual bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer;
        }

        public void CalculateMovement(Rectangle bounds, int ticks)
        {
            Point location = SpriteRectangle.Location;
            location.X += (MoveRight ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0) - (MoveLeft ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0);
            location.Y += (MoveDown  ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0) - (MoveUp   ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0);
            ProcessMovement(bounds, new Rectangle(location, SpriteRectangle.Size));
        }

        public virtual void ProcessMovement(Rectangle bounds, Rectangle proposedRectangle)
        {
            SpriteRectangle = proposedRectangle;
        }

        public double GetSpeed(int ticks)
        {
            int totalDistance = MaxSpeed * ticks;
            // If Entity is moving diagonally we must reduce speed for movement calculations. (1 up/down & 1 left/right is greater than 1 total unit of distance)
            double speed = ((Convert.ToInt32(MoveRight) + Convert.ToInt32(MoveLeft) + Convert.ToInt32(MoveUp) + Convert.ToInt32(MoveDown)) == 2) ? totalDistance / Math.Sqrt(2) : totalDistance;

            return Math.Round((double)speed, 0, MidpointRounding.ToEven);
        }
    }
}
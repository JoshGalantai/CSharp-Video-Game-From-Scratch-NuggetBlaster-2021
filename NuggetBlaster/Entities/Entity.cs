using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public virtual double BaseSpeed { get; set; } = 0.2;
        public virtual double SpeedMulti { get; set; } = 1.0;
        public virtual bool MoveRight { get; set; } = false;
        public virtual bool MoveLeft { get; set; } = false;
        public virtual bool MoveUp { get; set; } = false;
        public virtual bool MoveDown { get; set; } = false;
        public virtual bool Spacebar { get; set; } = false;
        public virtual int Team { get; set; } = 0;
        public virtual bool CanShoot { get; set; } = false;
        public virtual int ProjectileWidth { get; set; } = 0;
        public virtual int ProjectileHeight { get; set; } = 0;
        public virtual bool Damageable { get; set; } = true;
        public virtual int HitPoints { get; set; } = 1;
        public virtual long ShootCooldownTimer { get; set; } = 0;
        public virtual int ShootCooldownMS { get; set; } = 1500;
        public virtual int PointsOnKill { get; set; } = 0;
        public virtual Image SpriteOriginal { get; set; } = Resources.pickle;
        public virtual Image SpriteCached { get; set; } = Resources.pickle;
        public virtual Rectangle SpriteRectangle { get; set; } = new Rectangle(0, 0, 0, 0);
        public virtual Rectangle GameRectangle { get; set; } = new Rectangle(0, 0, 0, 0);


        public Entity(Rectangle gameRectangle, Rectangle spriteRectangle, Image sprite = null)
        {
            SpriteRectangle = spriteRectangle;
            GameRectangle   = gameRectangle;
            SpriteOriginal  = SpriteCached = GameForm.ResizeImage(sprite ?? SpriteOriginal, SpriteRectangle);

            ProjectileHeight = (int)(GameRectangle.Width * 0.0167);
            ProjectileWidth  = (int)(GameRectangle.Width * 0.03125);
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

        public void CalculateMovement(int ticks)
        {
            Point location = SpriteRectangle.Location;
            location.X += (MoveRight ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0) - (MoveLeft ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0);
            location.Y += (MoveDown  ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0) - (MoveUp   ? (int)GameCore.Engine.GetPPF(GetSpeed(ticks)) : 0);
            ProcessMovement(new Rectangle(location, SpriteRectangle.Size));
        }

        public virtual void ProcessMovement(Rectangle proposedRectangle)
        {
            SpriteRectangle = proposedRectangle;
        }

        public double GetSpeed(int ticks)
        {
            // If Entity is moving diagonally we must reduce speed for movement calculations. (1 up/down & 1 left/right is greater than 1 total unit of distance)
            double diagonalMovementModifier = ((Convert.ToInt32(MoveRight) + Convert.ToInt32(MoveLeft) + Convert.ToInt32(MoveUp) + Convert.ToInt32(MoveDown)) == 2) ? 1 / Math.Sqrt(2) : 1;
            double totalDistance            = BaseSpeed * SpeedMulti * ticks * GameRectangle.Width * diagonalMovementModifier;
           
            return Math.Round((double)totalDistance, 0, MidpointRounding.ToEven);
        }
    }
}
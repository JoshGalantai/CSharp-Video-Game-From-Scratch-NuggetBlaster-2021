using NuggetBlaster.Helpers;
using NuggetBlaster.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public double    BaseSpeed          = 0.2;
        public double    SpeedMulti         = 1.0;
        public bool      MoveRight          = false;
        public bool      MoveLeft           = false;
        public bool      MoveUp             = false;
        public bool      MoveDown           = false;
        public bool      Spacebar           = false;
        public int       Team               = 0;
        public bool      CanShoot           = false;
        public bool      Damageable         = true;
        public int       Damage             = 1;
        public int       HitPoints          = 1;
        public long      ShootCooldownTimer = 0;
        public int       ShootCooldownMS    = 1500;
        public int       PointsOnKill       = 0;
        public Image     SpriteOriginal     = Resources.pickle;
        public Image     SpriteCached;
        public Rectangle SpriteRectangle;
        public Rectangle GameRectangle;
        public int       ProjectileWidth;
        public int       ProjectileHeight;

        public Entity(Rectangle gameRectangle, Rectangle spriteRectangle, Image sprite = null)
        {
            SpriteRectangle = spriteRectangle;
            GameRectangle   = gameRectangle;
            SpriteOriginal  = SpriteCached = DrawManager.ResizeImage(sprite ?? SpriteOriginal, SpriteRectangle);

            ProjectileHeight = (int)(GameRectangle.Width * 0.0167);
            ProjectileWidth  = (int)(GameRectangle.Width * 0.03125);
        }

        public virtual List<ProjectileEntity> Shoot()
        {
            return new List<ProjectileEntity>();
        }

        protected void ShootCooldown()
        {
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS;
        }

        protected virtual bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer;
        }

        public virtual void TakeDamage(Entity entity)
        {
            if (Damageable)
                HitPoints -= entity.Damage;
        }

        public void CalculateMovement(int ticks)
        {
            Point location = SpriteRectangle.Location;
            location.X += (MoveRight ? (int)GameCore.Engine.ConvertPerSecondToPerFrame(GetDistanceTravelled(ticks)) : 0) - (MoveLeft ? (int)GameCore.Engine.ConvertPerSecondToPerFrame(GetDistanceTravelled(ticks)) : 0);
            location.Y += (MoveDown  ? (int)GameCore.Engine.ConvertPerSecondToPerFrame(GetDistanceTravelled(ticks)) : 0) - (MoveUp   ? (int)GameCore.Engine.ConvertPerSecondToPerFrame(GetDistanceTravelled(ticks)) : 0);
            ProcessMovement(new Rectangle(location, SpriteRectangle.Size));
        }

        protected virtual void ProcessMovement(Rectangle proposedRectangle)
        {
            SpriteRectangle = proposedRectangle;
        }

        /// <summary>
        /// Calculate distance of entity movement during a process phase
        /// Note: When moving diagonally we must calculate distance differently
        /// (1 unut up/down & 1 unit left/right is greater than 1 total unit of distance)
        /// </summary>
        private double GetDistanceTravelled(int ticks)
        {
            double diagonalMovementModifier = ((Convert.ToInt32(MoveRight) + Convert.ToInt32(MoveLeft) + Convert.ToInt32(MoveUp) + Convert.ToInt32(MoveDown)) == 2) ? 1 / Math.Sqrt(2) : 1;
            double totalDistance            = BaseSpeed * SpeedMulti * ticks * GameRectangle.Width * diagonalMovementModifier;
           
            return Math.Round((double)totalDistance, 0, MidpointRounding.ToEven);
        }
    }
}
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public virtual int  BaseSpeed  { get; set; } = 0;
        public virtual int  MaxSpeed   { get; set; } = 0;
        public virtual bool MoveRight  { get; set; } = false;
        public virtual bool MoveLeft   { get; set; } = false;
        public virtual bool MoveUp     { get; set; } = false;
        public virtual bool MoveDown   { get; set; } = false;
        public virtual bool Spacebar   { get; set; } = false;
        public virtual int Team        { get; set; } = 0;
        public virtual bool CanShoot   { get; set; } = false;
        public virtual bool Damageable { get; set; } = true;
        public virtual int HitPoints   { get; set; }  = 1;
        public virtual long ShootCooldownTimer { get; set; } = 0;
        public virtual int ShootCooldownMS { get; set; } = 2000;

        protected Entity()
        {
            MaxSpeed = BaseSpeed;
        }

        public abstract ProjectileEntity Shoot();


        public virtual dynamic CheckCollision()
        {
            return null;
        }

        public void ShootCooldown()
        {
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS;
        }

        public bool CheckCanShoot()
        {
            return (CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer);
        }

        public Point CalculateMovement(Point coordinates)
        {
            coordinates.X += (MoveRight ? GameCore.Engine.GetPPF(GetSpeed()) : 0) - (MoveLeft ? GameCore.Engine.GetPPF(GetSpeed()) : 0);
            CheckCollision();
            coordinates.Y += (MoveDown ? GameCore.Engine.GetPPF(GetSpeed()) : 0) - (MoveUp ? GameCore.Engine.GetPPF(GetSpeed()) : 0);
            CheckCollision();

            return coordinates;
        }

        public double GetSpeed()
        {
            double speed = ((
                Convert.ToInt32(MoveRight) +
                Convert.ToInt32(MoveLeft) +
                Convert.ToInt32(MoveUp) +
                Convert.ToInt32(MoveDown)
            ) == 2) ? MaxSpeed / Math.Sqrt(2) : MaxSpeed;

            return Math.Round((Double)speed, 0, MidpointRounding.ToEven);
        }
    }
}
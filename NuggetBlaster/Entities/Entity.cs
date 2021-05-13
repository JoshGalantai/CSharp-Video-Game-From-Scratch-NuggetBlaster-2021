using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public virtual int  baseSpeed  { get; set; } = 0;
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
            MaxSpeed = baseSpeed;
        }

        public void Spawn()
        {

        }

        public abstract ProjectileEntity Shoot();


        public void CheckCollision()
        {

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

        public Double GetSpeed()
        {
            double speed = ((
                Convert.ToInt32(this.MoveRight) +
                Convert.ToInt32(this.MoveLeft) +
                Convert.ToInt32(this.MoveUp) +
                Convert.ToInt32(this.MoveDown)
            ) == 2) ? this.MaxSpeed / Math.Sqrt(2) : this.MaxSpeed;

            return Math.Round((Double)speed, 0, MidpointRounding.ToEven);
        }
    }


}

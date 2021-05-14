using System;

namespace NuggetBlaster.Entities
{
    public class ProjectileEntity : Entity
    {
        public override int BaseSpeed { get; set; } = 400;

        public override ProjectileEntity Shoot()
        {
            throw new NotImplementedException();
        }

        public override dynamic CheckCollision()
        {
            return this;
        }
    }
}
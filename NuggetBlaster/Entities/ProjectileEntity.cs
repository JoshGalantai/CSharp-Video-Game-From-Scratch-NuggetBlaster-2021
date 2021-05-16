using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class ProjectileEntity : Entity
    {
        public ProjectileEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite) { }

        public override ProjectileEntity Shoot()
        {
            throw new NotImplementedException();
        }
    }
}
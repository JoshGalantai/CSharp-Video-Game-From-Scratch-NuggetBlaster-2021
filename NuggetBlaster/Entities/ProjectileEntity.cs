using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class ProjectileEntity : Entity
    {
        public ProjectileEntity(Rectangle GameCanvas, Rectangle spriteRectangle, Image sprite = null) : base(GameCanvas, spriteRectangle, sprite) { }

        public override ProjectileEntity Shoot()
        {
            throw new NotImplementedException();
        }
    }
}
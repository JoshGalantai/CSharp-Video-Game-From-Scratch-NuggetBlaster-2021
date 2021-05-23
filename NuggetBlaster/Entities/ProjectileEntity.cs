using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public class ProjectileEntity : Entity, ICloneable
    {
        public ProjectileEntity(Rectangle GameCanvas, Rectangle spriteRectangle, Image sprite = null) : base(GameCanvas, spriteRectangle, sprite) { }

        public object Clone()
        {
            return (ProjectileEntity)MemberwiseClone();
        }
    }
}
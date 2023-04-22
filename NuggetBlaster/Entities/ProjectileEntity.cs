namespace NuggetBlaster.Entities
{
  using System;
  using System.Drawing;

  public class ProjectileEntity : Entity, ICloneable
  {
    public ProjectileEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite)
    {
    }

    public object Clone()
    {
      return (ProjectileEntity)MemberwiseClone();
    }
  }
}
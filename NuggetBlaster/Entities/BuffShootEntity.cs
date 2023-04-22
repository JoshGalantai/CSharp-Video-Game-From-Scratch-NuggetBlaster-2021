namespace NuggetBlaster.Entities
{
  using System;
  using System.Drawing;
  using Properties;
  
  public class BuffShootEntity : BuffEntity
  {
    public BuffShootEntity(Random random, Image sprite = null) : base(random, sprite ?? Resources.buffShoot)
    {
    }

    public override PlayerEntity AddBuff(PlayerEntity entity)
    {
      if (entity.ShootBuffLevel < PlayerEntity.MaxShootBuffLevel)
      {
        entity.ShootBuffLevel++;
      }

      return entity;
    }
  }
}

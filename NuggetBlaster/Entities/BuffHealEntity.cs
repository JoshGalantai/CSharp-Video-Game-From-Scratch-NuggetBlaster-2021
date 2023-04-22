namespace NuggetBlaster.Entities
{
  using System;
  using System.Drawing;
  using GameCore;
  using Properties;

  public class BuffHealEntity : BuffEntity
  {
    public BuffHealEntity(Random random, Image sprite = null) : base(random, sprite ?? Resources.buffHeart)
    {
    }

    public override PlayerEntity AddBuff(PlayerEntity entity)
    {
      if (entity.HitPoints < EntityManager.MaxPlayerHP)
      {
        entity.HitPoints++;
      }

      return entity;
    }
  }
}

namespace NuggetBlaster.Entities
{
  using System;
  using System.Drawing;
  using Properties;
  using GameCore;

  public abstract class BuffEntity : Entity
  {
    public BuffEntity(Random random, Image sprite = null) : base(new Rectangle(Engine.GameAreaWidth, random.Next((int)((Engine.GameAreaHeight / 2) - (Engine.GameAreaHeight * 0.1)), (int)((Engine.GameAreaHeight / 2) + (Engine.GameAreaHeight * 0.1))), (int)(Engine.GameAreaWidth * 0.05), (int)(Engine.GameAreaWidth * 0.05)), sprite ?? Resources.buffHeart)
    {
      this.MoveLeft = true;
      this.Team = 1;
      this.Damageable = false;
      this.Damage = 0;
    }

    public abstract PlayerEntity AddBuff(PlayerEntity entity);
  }
}

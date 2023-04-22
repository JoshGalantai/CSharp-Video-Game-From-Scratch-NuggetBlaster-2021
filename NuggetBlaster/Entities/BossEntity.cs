namespace NuggetBlaster.Entities
{
  using System.Drawing;
  using Properties;

  public class BossEntity : EnemyEntity
  {
    public const int MaxHP = 50;

    public BossEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite ?? Resources.bossPickle)
    {
      this.PointsOnKill = 50000;
      this.ShootCooldownMS = 2500;
      this.HitPoints = MaxHP;
      this.BaseSpeed = 0.250;
      this.IsDamagedOnTouch = false;
      this.AllowHorizontalExit = false;
      this.TripleShot = true;
      this.CanShoot = true;
    }
  }
}
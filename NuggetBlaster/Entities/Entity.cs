namespace NuggetBlaster.Entities
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using Helpers;
  using Properties;
  using GameCore;

  public enum EntitySpriteType
  {
    Player,
    Boss,
    ProjectileAlly,
    ProjectileAllySuper,
    ProjectileEnemy,
    EnemyLvlOne,
    EnemyLvlTwo,
    EnemyLvlThree,
    BuffHeal,
    BuffShoot
  };

  public abstract class Entity
  {
    public Entity(Rectangle spriteRectangle, Image sprite = null)
    {
      this.SpriteRectangle = spriteRectangle;
      this.SpriteOriginal = this.SpriteCached = DrawManager.ResizeImage(sprite ?? this.SpriteOriginal, this.SpriteRectangle);
    }

    public double BaseSpeed { get; set; } = 0.2;

    public double SpeedMulti { get; set; } = 1.0;

    public bool MoveRight { get; set; } = false;

    public bool MoveLeft { get; set; } = false;

    public bool MoveUp { get; set; } = false;

    public bool MoveDown { get; set; } = false;

    public bool Spacebar { get; set; } = false;

    public int Team { get; set; } = 0;

    public bool CanShoot { get; set; } = false;

    public bool Damageable { get; set; } = true;

    public int Damage { get; set; } = 1;

    public int HitPoints { get; set; } = 1;

    public long ShootCooldownTimer { get; set; } = 0;

    public int ShootCooldownMS { get; set; } = 1500;

    public int PointsOnKill { get; set; } = 0;

    public Image SpriteOriginal { get; set; } = Resources.pickle;

    public Image SpriteCached { get; set; }

    public Rectangle SpriteRectangle { get; set; }

    public int ProjectileWidth { get; set; }

    public int ProjectileHeight { get; set; }

    public EntitySpriteType EntitySpriteType { get; set; }

    public virtual List<ProjectileEntity> Shoot()
    {
      return new List<ProjectileEntity>();
    }

    public virtual void TakeDamage(Entity entity)
    {
      if (this.Damageable)
      {
        this.HitPoints -= entity.Damage;
      }
    }

    public void CalculateMovement(int ticks)
    {
      Point location = this.SpriteRectangle.Location;
      location.X += (this.MoveRight ? (int)Engine.ConvertPerSecondToPerFrame(this.GetDistanceTravelled(ticks)) : 0) - (this.MoveLeft ? (int)Engine.ConvertPerSecondToPerFrame(this.GetDistanceTravelled(ticks)) : 0);
      location.Y += (this.MoveDown ? (int)Engine.ConvertPerSecondToPerFrame(this.GetDistanceTravelled(ticks)) : 0) - (this.MoveUp ? (int)Engine.ConvertPerSecondToPerFrame(this.GetDistanceTravelled(ticks)) : 0);
      this.ProcessMovement(new Rectangle(location, this.SpriteRectangle.Size));
    }

    protected void ShootCooldown()
    {
      this.ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + this.ShootCooldownMS;
    }

    protected virtual bool CheckCanShoot()
    {
      return this.CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > this.ShootCooldownTimer;
    }

    protected virtual void ProcessMovement(Rectangle proposedRectangle)
    {
      this.SpriteRectangle = proposedRectangle;
    }

    /// <summary>
    /// Calculate distance of entity movement during a process phase
    /// Note: When moving diagonally we must calculate distance differently
    /// (1 unut up/down & 1 unit left/right is greater than 1 total unit of distance)
    /// </summary>
    private double GetDistanceTravelled(int ticks)
    {
      double diagonalMovementModifier = ((Convert.ToInt32(this.MoveRight) + Convert.ToInt32(this.MoveLeft) + Convert.ToInt32(this.MoveUp) + Convert.ToInt32(this.MoveDown)) == 2) ? 1 / Math.Sqrt(2) : 1;
      double totalDistance = this.BaseSpeed * this.SpeedMulti * ticks * Engine.GameAreaWidth * diagonalMovementModifier;

      return Math.Round((double)totalDistance, 0, MidpointRounding.ToEven);
    }
  }
}
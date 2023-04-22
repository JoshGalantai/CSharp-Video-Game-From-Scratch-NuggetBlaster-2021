namespace NuggetBlaster.Entities
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using Properties;
  using GameCore;

  public class PlayerEntity : Entity
  {
    public const int MaxShootBuffLevel = 4; // Highest buff level allowed
    public int ShootBuffLevel { get; set; } = 0; // Each buff level enhances players shooting
    public long DamageableCooldownTimer { get; set; } = 0;
    private int DamageableCooldownMS { get; set; } = 1000; // After getting hit player is invulnerable for a brief period

    public PlayerEntity() : base(new Rectangle(0, (int)(Engine.GameAreaHeight * 0.5), (int)(Engine.GameAreaWidth * 0.1), (int)(Engine.GameAreaWidth * 0.05)), Resources.nugget)
    {
      BaseSpeed = 0.2;
      Team = 1;
      CanShoot = true;
      ShootCooldownMS = 400;
    }

    public override void TakeDamage(Entity entity)
    {
      if (!Damageable || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < DamageableCooldownTimer)
      {
        return;
      }

      HitPoints -= entity.Damage;
      if (ShootBuffLevel > 0)
      {
        ShootBuffLevel--;
      }

      DamageableCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + DamageableCooldownMS;
    }

    protected override bool CheckCanShoot()
    {
      return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer && Spacebar;
    }

    public override List<ProjectileEntity> Shoot()
    {
      List<ProjectileEntity> projList = new();
      if (CheckCanShoot())
      {
        ShootCooldown();

        int height     = (int)(EntityManager.ProjectileEntityHeight * Engine.GameAreaHeight);
        int width      = (int)(EntityManager.ProjectileEntityWidth * Engine.GameAreaWidth);
        int halfHeight = height / 2;

        int x = SpriteRectangle.Right + 20;
        int y = SpriteRectangle.Top + (SpriteRectangle.Height / 2) - halfHeight;
        var sprite = ShootBuffLevel >= 4 ? Resources.allyProjectileSuper : Resources.allyProjectile;
        var multi = ShootBuffLevel >= 4 ? 3.0 : 2.0;

        // Buff level 1 shoots two projectiles - provide a vertical offset to maintain centering on player
        int verticalOffset = ShootBuffLevel == 1 ? halfHeight : 0;

        ProjectileEntity projectile = new(new Rectangle(new Point(x, y + verticalOffset), new Size(width, height)), sprite)
        {
          MoveRight = true,
          BaseSpeed = BaseSpeed,
          SpeedMulti = multi,
          Team = Team,
          Damage = Damage
        };
        projList.Add(projectile);

        // First buff level adds a second projectile with vertical offset equal to projectile height
        if (ShootBuffLevel >= 1)
        {
          ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
          proj.SpriteRectangle = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y - height), projectile.SpriteRectangle.Size);
          projList.Add(proj);
        }
        // Second buff level adds a third projectile with vertical offset equal to projectile height
        if (ShootBuffLevel >= 2)
        {
          ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
          proj.SpriteRectangle = new Rectangle(new Point(projectile.SpriteRectangle.X, projectile.SpriteRectangle.Y + height), projectile.SpriteRectangle.Size);
          projList.Add(proj);
        }
        // Third buff level adds a fourth and fifth projectile fired diagonally
        if (ShootBuffLevel >= 3)
        {
          ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
          proj.MoveUp = true;
          projList.Add(proj);

          proj = (ProjectileEntity)projectile.Clone();
          proj.MoveDown = true;
          projList.Add(proj);
        }
      }
      return projList;
    }

    // Make sure player does not go out of bounds
    protected override void ProcessMovement(Rectangle proposedRectangle)
    {
      int x = proposedRectangle.X < 0 ? 0 : proposedRectangle.X;
      int y = proposedRectangle.Y < 0 ? 0 : proposedRectangle.Y;
      x = proposedRectangle.X > Engine.GameAreaWidth - proposedRectangle.Width ? Engine.GameAreaWidth - proposedRectangle.Width : x;
      y = proposedRectangle.Y > Engine.GameAreaHeight - proposedRectangle.Height ? Engine.GameAreaHeight - proposedRectangle.Height : y;

      SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
    }
  }
}
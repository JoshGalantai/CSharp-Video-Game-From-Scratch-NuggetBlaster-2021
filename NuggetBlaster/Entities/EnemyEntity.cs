namespace NuggetBlaster.Entities
{
  using GameCore;
  using Properties;
  using System;
  using System.Collections.Generic;
  using System.Drawing;

  public class EnemyEntity : Entity
  {
    protected bool IsDamagedOnTouch = true;  // Entity only takes damage from projectiles
    protected bool AllowHorizontalExit = true;  // Entity exits left or right instead of "bouncing" off
    protected bool TripleShot = false; // Entity has alternate fire pattern (3 shots)

    public EnemyEntity(Rectangle spriteRectangle, Image sprite = null) : base(spriteRectangle, sprite)
    {
      MoveLeft = true;
      Team = 2;
      PointsOnKill = 100;
      ShootCooldownMS = 2000;
      ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS / 10; // Slight delay before first shot after spawn
    }

    public override List<ProjectileEntity> Shoot()
    {
      List<ProjectileEntity> projList = new();
      if (CheckCanShoot())
      {
        ShootCooldown();
        Point location = new(SpriteRectangle.Left - 20, SpriteRectangle.Top + (SpriteRectangle.Height / 2) - (ProjectileHeight / 2));
        ProjectileEntity projectile = new(new Rectangle(location, new Size(ProjectileWidth, ProjectileHeight)), Resources.enemyProjectile)
        {
          MoveLeft = true,
          BaseSpeed = BaseSpeed * SpeedMulti,
          SpeedMulti = 1.3,
          Team = Team,
          Damage = Damage
        };
        projList.Add(projectile);
        if (TripleShot)
        {
          ProjectileEntity proj = (ProjectileEntity)projectile.Clone();
          proj.MoveUp = true;
          proj.SpeedMulti = 1.1;
          projList.Add(proj);

          proj = (ProjectileEntity)projectile.Clone();
          proj.MoveDown = true;
          proj.SpeedMulti = 1.1;
          projList.Add(proj);
        }
      }
      return projList;
    }

    public override void TakeDamage(Entity entity)
    {
      if (Damageable && (IsDamagedOnTouch || entity.GetType() == typeof(ProjectileEntity)))
      {
        HitPoints -= entity.Damage;
      }
    }

    /// <summary>
    /// Process entity movement - Optionally enemies can "bounce" off game bounds depending on entity config
    /// </summary>
    protected override void ProcessMovement(Rectangle proposedRectangle)
    {
      // "Bounce" off left and right of game area
      int x = proposedRectangle.X < 0 && !AllowHorizontalExit ? 0 : proposedRectangle.X;
      if (x == 0 && MoveLeft && !AllowHorizontalExit)
      {
        MoveRight = true;
        MoveLeft = false;
      }
      x = proposedRectangle.X > Engine.GameAreaWidth - proposedRectangle.Width && !AllowHorizontalExit ? Engine.GameAreaWidth - proposedRectangle.Width : x;
      if (x == Engine.GameAreaWidth - proposedRectangle.Width && MoveRight && !AllowHorizontalExit)
      {
        MoveRight = false;
        MoveLeft = true;
      }

      // "Bounce" off top and bottom of game area
      int y = proposedRectangle.Y < 0 ? 0 : proposedRectangle.Y;
      if (y == 0 && MoveUp)
      {
        MoveUp = false;
        MoveDown = true;
      }
      y = proposedRectangle.Y > Engine.GameAreaHeight - proposedRectangle.Height ? Engine.GameAreaHeight - proposedRectangle.Height : y;
      if (y == Engine.GameAreaHeight - proposedRectangle.Height && MoveDown)
      {
        MoveUp = true;
        MoveDown = false;
      }

      SpriteRectangle = new Rectangle(new Point(x, y), proposedRectangle.Size);
    }
  }
}
namespace NuggetBlaster.GameCore
{
  using Entities;
  using Properties;
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;

  class EntityManager
  {
    private readonly Engine _gameEngine;
    private readonly Random _random = new();

    private readonly IDictionary<string, string> _entitySoundEffectsList = new Dictionary<string, string>();
    public readonly IDictionary<string, Entity> EntityDataList = new Dictionary<string, Entity>();

    // Entity Config
    public const int MaxPlayerHP = 5;
    private const int _enemySpawnCooldownMS = 300;
    private const int _powerUpSpawnInterval = 7500;
    private const int _heartPointsSpawnInterval = 10000;

    // Entity Vars
    private int _nextPowerUpSpawnPoints;
    private int _nextHeartSpawnPoints;
    private long _enemySpawnCooldownTimer;
    private int _entityIterator;
    private int _enemyCount;
    public bool SpawnBoss;
    public double EnemySpeedMulti;
    public int MaxEnemies;

    // Entity Dimensions (Relative to Game Area)
    public const double PlayerEntityWidth = 0.1;
    public const double PlayerEntityHeight = 0.05;
    public const double EnemyEntityWidth = 0.1; // width = height
    public const double BossEntityWidth = 0.2;
    public const double BossEntityHeight = 0.2;
    public const double BuffEntityWidth = 0.05;
    public const double BuffEntityHeight = 0.05;
    public const double ProjectileEntityWidth = 0.02;
    public const double ProjectileEntityHeight = 0.04;

    public EntityManager(Engine gameEngine)
    {
      _gameEngine = gameEngine;

      // Async audio player needed for SFX cannot read from resource - Copy to local
      _entitySoundEffectsList["boom"] = Engine.GetResourceAsLocal(Resources.boom, "boom.wav");
      _entitySoundEffectsList["shoot"] = Engine.GetResourceAsLocal(Resources.shoot, "shoot.wav");
      _entitySoundEffectsList["heal"] = Engine.GetResourceAsLocal(Resources.heal, "heal.wav");
      _entitySoundEffectsList["oof"] = Engine.GetResourceAsLocal(Resources.oof, "oof.wav");
      _entitySoundEffectsList["upgrade"] = Engine.GetResourceAsLocal(Resources.upgrade, "upgrade.wav");
    }

    /// <summary>
    /// Reset entity related game values to defaults
    /// </summary>
    public void Reset()
    {
      SpawnBoss = false;

      _nextHeartSpawnPoints = _heartPointsSpawnInterval;
      _nextPowerUpSpawnPoints = _powerUpSpawnInterval;

      if (!EntityDataList.ContainsKey("player"))
      {
        EntityDataList["player"] = new PlayerEntity();
      }
      EntityDataList["player"].HitPoints = MaxPlayerHP;
    }

    /// <summary>
    /// Process creation of entities - Modify game values accordingly
    /// </summary>
    public void ProcessEntityCreation()
    {
      if (!_gameEngine.IsRunning)
      {
        return;
      }
      SpawnBoss = true;
      if (SpawnBoss)
      {
        if (!EntityDataList.ContainsKey("boss"))
        {
          EntityDataList["boss"] = GetBossEntity();
        }
        SpawnBoss = false;
      }
      if (_enemyCount < MaxEnemies && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > _enemySpawnCooldownTimer)
      {
        EnemyEntity enemy = GetRandomEnemyEntity();
        AddEntity(enemy);
      }
      if (_gameEngine.Score > _nextHeartSpawnPoints)
      {
        BuffHealEntity buffHeal = new(_random);
        _nextHeartSpawnPoints += _heartPointsSpawnInterval;
        AddEntity(buffHeal);
      }
      if (_gameEngine.Score > _nextPowerUpSpawnPoints)
      {
        BuffShootEntity buffShoot = new(_random);
        _nextPowerUpSpawnPoints += _powerUpSpawnInterval;
        AddEntity(buffShoot);
      }
    }

    /// <summary>
    /// Process entity movement (And interaction with GameArea bounderies)
    /// </summary>
    public void ProcessEntityMovement()
    {
      if (!_gameEngine.IsRunning)
      {
        return;
      }

      Rectangle gameArea = new(0, 0, Engine.GameAreaWidth, Engine.GameAreaHeight);
      foreach (string key in EntityDataList.Keys.ToArray())
      {
        EntityDataList[key].CalculateMovement(_gameEngine.TicksToProcess);
        if (!Engine.RectangleOverlaps(gameArea, EntityDataList[key].SpriteRectangle))
        {
          DeleteEntity(key);
        }
      }
    }

    /// <summary>
    /// Process entity "shooting" (creation of new projectile entities)
    /// </summary>
    public void ProcessProjectileCreation()
    {
      if (!_gameEngine.IsRunning)
      {
        return;
      }
      bool playShoot = false;
      string[] entityKeys = EntityDataList.Keys.ToArray();
      foreach (string key in entityKeys)
      {
        var projectiles = EntityDataList[key].Shoot();
        if (projectiles.Count > 0 && EntityDataList[key].GetType() != typeof(EnemyEntity))
        {
          playShoot = true;
        }
        foreach (ProjectileEntity entity in projectiles)
        {
          AddEntity(entity);
        }
      }
      if (playShoot)
      {
        Engine.PlaySoundAsync(_entitySoundEffectsList["shoot"]);
      }
    }

    /// <summary>
    /// Process contact between varying entity types (Damage, Buffs, etc.)
    /// </summary>
    public void ProcessEntityCollisions()
    {
      if (!_gameEngine.IsRunning)
      {
        return;
      }
      int enemyCountStart = _enemyCount;
      int playerHPStart = GetPlayerHP();
      string[] entityKeys = EntityDataList.Keys.ToArray();
      foreach (string targetKey in entityKeys)
      {
        if (!EntityDataList.ContainsKey(targetKey) || EntityDataList[targetKey].GetType() == typeof(ProjectileEntity))
        {
          continue;
        }
        foreach (string comparisonKey in entityKeys)
        {
          if (comparisonKey == targetKey || !EntityDataList.ContainsKey(comparisonKey) || !EntityDataList.ContainsKey(targetKey))
          {
            continue;
          }
          // Check if compared entities' sprite rectangles overlap 
          if (Engine.RectangleOverlaps(EntityDataList[targetKey].SpriteRectangle, EntityDataList[comparisonKey].SpriteRectangle))
          {
            // If compared entities are not same team, process damage
            if (EntityDataList[comparisonKey].Team != EntityDataList[targetKey].Team)
            {
              EntityDataList[targetKey].TakeDamage(EntityDataList[comparisonKey]);
              EntityDataList[comparisonKey].TakeDamage(EntityDataList[targetKey]);

              if (EntityDataList[targetKey].HitPoints < 1)
              {
                DeleteEntity(targetKey, true);
              }
              if (EntityDataList[comparisonKey].HitPoints < 1)
              {
                DeleteEntity(comparisonKey, true);
              }
              // Check if player interacts with a buff entity (Heal, Upgrade, etc.)
            } else if (EntityDataList[targetKey].GetType() == typeof(PlayerEntity) && EntityDataList[comparisonKey].GetType().IsSubclassOf(typeof(BuffEntity)))
            {
              EntityDataList[targetKey] = (EntityDataList[comparisonKey] as BuffEntity).AddBuff(EntityDataList[targetKey] as PlayerEntity);
              if (EntityDataList[comparisonKey].GetType() == typeof(BuffHealEntity))
              {
                Engine.PlaySoundAsync(_entitySoundEffectsList["heal"]);
              }
              if (EntityDataList[comparisonKey].GetType() == typeof(BuffShootEntity))
              {
                Engine.PlaySoundAsync(_entitySoundEffectsList["upgrade"]);
              }

              DeleteEntity(comparisonKey, true);
            }
          }
        }
      }
      if (GetPlayerHP() < playerHPStart)
      {
        Engine.PlaySoundAsync(_entitySoundEffectsList["oof"]);
      } else if (_enemyCount < enemyCountStart)
      {
        Engine.PlaySoundAsync(_entitySoundEffectsList["boom"]);
      }
    }

    /// <summary>
    /// Remove all existing entities - Modify game values accordingly
    /// </summary>
    public void ClearEntities()
    {
      foreach (KeyValuePair<string, Entity> entity in EntityDataList)
      {
        DeleteEntity(entity.Key);
      }

      EntityDataList.Clear();
      _enemyCount = 0;
      _entityIterator = 0;
    }

    /// <summary>
    /// Handle adding new entity - Modify game values accordingly
    /// </summary>
    private void AddEntity(Entity Data)
    {
      if (Data.GetType() != typeof(EnemyEntity) || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > _enemySpawnCooldownTimer)
      {
        EntityDataList[_entityIterator.ToString()] = Data;
        _entityIterator++;
        if (Data.GetType() == typeof(EnemyEntity))
        {
          _enemyCount++;
          _enemySpawnCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _enemySpawnCooldownMS;
        }
      }
    }

    /// <summary>
    /// Handle deletion of entities - Modify game values accordingly
    /// </summary>
    private void DeleteEntity(string id, bool addPoints = false)
    {
      if (!EntityDataList.ContainsKey(id))
      {
        return;
      }
      if (EntityDataList[id].GetType() == typeof(PlayerEntity))
      {
        _gameEngine.IsRunning = false;
      }
      if (EntityDataList[id].GetType() == typeof(EnemyEntity))
      {
        _enemyCount--;
      }
      if (addPoints)
      {
        _gameEngine.Score += EntityDataList[id].PointsOnKill;
      }
      EntityDataList.Remove(id);
    }

    /// <summary>
    /// Return (weighted) random enemy entity
    /// </summary>
    private EnemyEntity GetRandomEnemyEntity()
    {
      int enemySeed = _random.Next(0, 10);

      if (enemySeed > 8)
      {
        return _gameEngine.GameStage >= 3 ? GetStageThreeEnemyEntity() : GetStageOneEnemyEntity();
      } else if (enemySeed > 4)
      {
        return _gameEngine.GameStage >= 2 ? GetStageTwoEnemyEntity() : GetStageOneEnemyEntity();
      } else
      {
        return GetStageOneEnemyEntity();
      }
    }

    /// <summary>
    /// Return default enemy location and dimensions for standard enemies
    /// </summary>
    private Rectangle GetEnemyEntityRectangle()
    {
      int x = Engine.GameAreaWidth;
      int y = _random.Next(10, (int)(Engine.GameAreaHeight * 0.9));
      int w = (int)(Engine.GameAreaWidth * EnemyEntityWidth);
      return new Rectangle(x, y, w, w);
    }

    /// <summary>
    /// Simple left moving enemy - Does not shoot
    /// </summary>
    private EnemyEntity GetStageOneEnemyEntity()
    {
      EnemyEntity entity = new(GetEnemyEntityRectangle(), Resources.pickle);
      entity.BaseSpeed = _random.Next(400, 600) / 1000.0;
      entity.SpeedMulti = EnemySpeedMulti;
      entity.HitPoints = 1;
      return entity;
    }

    /// <summary>
    /// Same as Stage 1 except also shoots
    /// </summary>
    private EnemyEntity GetStageTwoEnemyEntity()
    {
      EnemyEntity entity = new(GetEnemyEntityRectangle(), Resources.coolPickle);
      entity.CanShoot = true;
      entity.BaseSpeed = _random.Next(200, 400) / 1000.0;
      entity.SpeedMulti = EnemySpeedMulti;
      entity.HitPoints = 2;
      entity.PointsOnKill = 300;
      return entity;
    }

    /// <summary>
    /// Same as Stage 2 except also moves diagonally up and down across screen
    /// </summary>
    private EnemyEntity GetStageThreeEnemyEntity()
    {
      EnemyEntity entity = new(GetEnemyEntityRectangle(), Resources.coolestPickle);
      entity.CanShoot = true;
      entity.BaseSpeed = _random.Next(200, 300) / 1000.0;
      entity.SpeedMulti = EnemySpeedMulti;
      entity.HitPoints = 3;
      entity.PointsOnKill = 600;
      entity.MoveUp = _random.Next(0, 1) == 1;
      entity.MoveDown = !entity.MoveUp;
      return entity;
    }

    /// <summary>
    /// Moves diagonally across screen bouncing on edges - Shoots in modified pattern
    /// </summary>
    private BossEntity GetBossEntity()
    {
      int w = (int)(Engine.GameAreaWidth * BossEntityWidth);
      int h = (int)(Engine.GameAreaHeight * BossEntityHeight);
      int x = Engine.GameAreaWidth - w;
      int y = Engine.GameAreaHeight - h;

      BossEntity entity = new(new Rectangle(x, y, w, h));
      entity.MoveUp = _random.Next(0, 1) == 1;
      entity.MoveDown = !entity.MoveUp;
      return entity;
    }

    #region Utility Functions

    /// <summary>
    /// Store Resized/Modified sprite image in entity object
    /// </summary>
    public void CacheResizedEntitySprite(Image image, string key)
    {
      if (!EntityDataList.ContainsKey(key))
      {
        return;
      }

      EntityDataList[key].SpriteCached = image;
    }

    /// <summary>
    /// Return list of entity sprite rectangles (Not scaled to UI resolution - Must be scaled on draw phase)
    /// </summary>
    public IDictionary<string, Rectangle> GetEntityRectangleList()
    {
      IDictionary<string, Rectangle> rectangleList = new Dictionary<string, Rectangle>();
      foreach (KeyValuePair<string, Entity> entity in EntityDataList)
      {
        rectangleList[entity.Key] = entity.Value.SpriteRectangle;
      }
      return rectangleList;
    }

    /// <summary>
    /// Return cached entity sprite list - Pass true to get original sprite (pre-resized / modified)
    /// </summary>
    public IDictionary<string, Image> GetEntitySpriteList(bool getOriginal = false)
    {
      IDictionary<string, Image> spriteList = new Dictionary<string, Image>();
      foreach (KeyValuePair<string, Entity> entity in EntityDataList)
      {
        spriteList[entity.Key] = getOriginal ? entity.Value.SpriteOriginal : entity.Value.SpriteCached;
      }
      return spriteList;
    }

    /// <summary>
    /// Return player HP - Or 0 if player not spawned
    /// </summary>
    public int GetPlayerHP()
    {
      return EntityDataList.ContainsKey("player") ? EntityDataList["player"].HitPoints : 0;
    }

    /// <summary>
    /// Return if player is currently in invincibility timer - Or false if player not spawned
    /// </summary>
    public bool IsPlayerInvulnerable()
    {
      if (!EntityDataList.ContainsKey("player"))
      {
        return false;
      }
      return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < (EntityDataList["player"] as PlayerEntity).DamageableCooldownTimer;
    }

    /// <summary>
    /// Return boss health percent eg. 70 - Or 0 if boss not spawned
    /// </summary>
    public int GetBossHealthPercent()
    {
      if (!EntityDataList.ContainsKey("boss"))
      {
        return 0;
      }
      return (int)((double)EntityDataList["boss"].HitPoints / BossEntity.MaxHP * 100);
    }

    #endregion
  }
}

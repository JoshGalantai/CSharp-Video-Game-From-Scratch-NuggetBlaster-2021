using NuggetBlaster.Entities;
using NuggetBlaster.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace NuggetBlaster.GameCore
{
    class EntityManager
    {
        private readonly Engine    GameEngine;
        private          Rectangle GameArea;
        private readonly Random    Random = new();

        private readonly IDictionary<string, string> EntitySoundEffectsList = new Dictionary<string, string>();
        public  readonly IDictionary<string, Entity> EntityDataList         = new Dictionary<string, Entity>();

        // Entity Config
        public const int MaxPlayerHP              = 5;
        public const int EnemySpawnCooldownMS     = 300;
        public const int PowerUpSpawnInterval     = 7500;
        public const int HeartPointsSpawnInterval = 10000;

        // Entity Vars
        public int    NextPowerUpSpawnPoints;
        public int    NextHeartSpawnPoints;
        public long   EnemySpawnCooldownTimer;
        public double EnemySpeedMulti;
        public int    MaxEnemies;
        public int    EntityIterator;
        public int    EnemyCount;
        public bool   SpawnBoss;

        public EntityManager(Engine gameEngine)
        {
            GameEngine = gameEngine;
        }

        public void Reset(Rectangle gameArea)
        {
            GameArea = gameArea;

            SpawnBoss = false;

            NextHeartSpawnPoints   = HeartPointsSpawnInterval;
            NextPowerUpSpawnPoints = PowerUpSpawnInterval;

            if (!EntityDataList.ContainsKey("player"))
                EntityDataList["player"] = new PlayerEntity(GameArea);
            EntityDataList["player"].HitPoints = MaxPlayerHP;

            // Async audio player needed for SFX cannot read from resource - Copy to local
            EntitySoundEffectsList["boom"]    = Engine.GetResourceAsLocal(Resources.boom,    "boom.wav");
            EntitySoundEffectsList["shoot"]   = Engine.GetResourceAsLocal(Resources.shoot,   "shoot.wav");
            EntitySoundEffectsList["heal"]    = Engine.GetResourceAsLocal(Resources.heal,    "heal.wav");
            EntitySoundEffectsList["oof"]     = Engine.GetResourceAsLocal(Resources.oof,     "oof.wav");
            EntitySoundEffectsList["upgrade"] = Engine.GetResourceAsLocal(Resources.upgrade, "upgrade.wav");
        }

        public void ProcessEntityCreation()
        {
            if (!GameEngine.IsRunning)
                return;
            if (SpawnBoss)
            {
                if (!EntityDataList.ContainsKey("boss"))
                    EntityDataList["boss"] = GetBossEntity();
                SpawnBoss = false;
            }
            if (EnemyCount < MaxEnemies)
            {
                EnemyEntity enemy = GetRandomEnemyEntity();
                AddEntity(enemy, "enemy-");
            }
            if (GameEngine.Score > NextHeartSpawnPoints)
            {
                BuffEntity buff = new(GameArea, Random);
                NextHeartSpawnPoints += HeartPointsSpawnInterval;
                AddEntity(buff, "buff-");
            }
            if (GameEngine.Score > NextPowerUpSpawnPoints)
            {
                BuffShootEntity buff = new(GameArea, Random);
                NextPowerUpSpawnPoints += PowerUpSpawnInterval;
                AddEntity(buff, "buff-");
            }
        }
 
        public void ProcessEntityMovement()
        {
            if (!GameEngine.IsRunning)
                return;
            foreach (string key in EntityDataList.Keys.ToArray())
            {
                EntityDataList[key].CalculateMovement(GameEngine.TicksToProcess);
                if (!Engine.RectangleOverlaps(EntityDataList[key].SpriteRectangle, GameArea))
                    DeleteEntity(key);
            }
        }

        public void ProcessProjectileCreation()
        {
            if (!GameEngine.IsRunning)
                return;
            bool playShoot = false;
            string[] entityKeys = EntityDataList.Keys.ToArray();
            foreach (string key in entityKeys)
            {
                var projectiles = EntityDataList[key].Shoot();
                if (projectiles.Count > 0 && EntityDataList[key].GetType() != typeof(EnemyEntity))
                    playShoot = true;
                foreach (ProjectileEntity entity in projectiles)
                    AddEntity(entity, "proj-");
            }
            if (playShoot)
                Engine.PlaySoundAsync(EntitySoundEffectsList["shoot"]);
        }

        public void ProcessEntityCollissions()
        {
            if (!GameEngine.IsRunning)
                return;
            int enemyCountStart = EnemyCount;
            int playerHPStart   = GetPlayerHP();
            string[] entityKeys = EntityDataList.Keys.ToArray();
            foreach (string targetKey in entityKeys)
            {
                if (!EntityDataList.ContainsKey(targetKey) || EntityDataList[targetKey].GetType() == typeof(ProjectileEntity))
                    continue;
                foreach (string comparisonKey in entityKeys)
                {
                    if (comparisonKey == targetKey || !EntityDataList.ContainsKey(comparisonKey) || !EntityDataList.ContainsKey(targetKey))
                        continue;
                    if (Engine.RectangleOverlaps(EntityDataList[targetKey].SpriteRectangle, EntityDataList[comparisonKey].SpriteRectangle))
                    {
                        if (EntityDataList[comparisonKey].Team != EntityDataList[targetKey].Team)
                        {
                            EntityDataList[targetKey].TakeDamage(EntityDataList[comparisonKey]);
                            EntityDataList[comparisonKey].TakeDamage(EntityDataList[targetKey]);

                            if (EntityDataList[targetKey].HitPoints < 1)
                                DeleteEntity(targetKey, true);
                            if (EntityDataList[comparisonKey].HitPoints < 1)
                                DeleteEntity(comparisonKey, true);
                        } else if (EntityDataList[targetKey].GetType() == typeof(PlayerEntity) && (EntityDataList[comparisonKey].GetType().IsSubclassOf(typeof(BuffEntity)) || EntityDataList[comparisonKey].GetType() == typeof(BuffEntity)))
                        {
                            EntityDataList[targetKey] = (EntityDataList[comparisonKey] as BuffEntity).AddBuff(EntityDataList[targetKey] as PlayerEntity);
                            if (EntityDataList[comparisonKey].GetType() == typeof(BuffEntity))
                                Engine.PlaySoundAsync(EntitySoundEffectsList["heal"]);
                            if (EntityDataList[comparisonKey].GetType() == typeof(BuffShootEntity))
                                Engine.PlaySoundAsync(EntitySoundEffectsList["upgrade"]);

                            DeleteEntity(comparisonKey, true);
                        }
                    }
                }
            }
            if (GetPlayerHP() < playerHPStart)
                Engine.PlaySoundAsync(EntitySoundEffectsList["oof"]);
            else if (EnemyCount < enemyCountStart)
                Engine.PlaySoundAsync(EntitySoundEffectsList["boom"]);
        }

        public void ClearEntities()
        {
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
                DeleteEntity(entity.Key);

            EntityDataList.Clear();
            EnemyCount     = 0;
            EntityIterator = 0;
        }

        public void AddEntity(Entity Data, string prefix = "")
        {
            if (Data.GetType() != typeof(EnemyEntity) || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > EnemySpawnCooldownTimer)
            {
                EntityDataList[prefix + EntityIterator.ToString()] = Data;
                EntityIterator++;
                if (Data.GetType() == typeof(EnemyEntity))
                {
                    EnemyCount++;
                    EnemySpawnCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + EnemySpawnCooldownMS;
                }
            }
        }
 
        public void DeleteEntity(string id, bool addPoints = false)
        {
            if (!EntityDataList.ContainsKey(id))
                return;
            if (EntityDataList[id].GetType() == typeof(PlayerEntity))
                GameEngine.IsRunning = false;
            if (EntityDataList[id].GetType() == typeof(EnemyEntity))
                EnemyCount--;
            if (addPoints)
                GameEngine.Score += EntityDataList[id].PointsOnKill;
            EntityDataList.Remove(id);
        }

        public EnemyEntity GetRandomEnemyEntity()
        {
            int enemySeed = Random.Next(0, 100);
            if (enemySeed > 80)
                return GetStageThreeEnemyEntity();
            else if (enemySeed > 40)
                return GetStageTwoEnemyEntity();
            else
                return GetStageOneEnemyEntity();
        }

        public Rectangle GetEnemyEntityRectangle()
        {
            int x = GameArea.Width;
            int y = Random.Next(10, GameArea.Height - (int)(GameArea.Width * 0.1));
            int w = (int)(GameArea.Width * 0.1);
            int h = (int)(GameArea.Width * 0.1);
            return new Rectangle(x, y, w, h);
        }

        public EnemyEntity GetStageOneEnemyEntity()
        {
            EnemyEntity entity = new(GameArea, GetEnemyEntityRectangle(), Resources.pickle);
            entity.BaseSpeed   = Random.Next(400, 600)/1000.0;
            entity.SpeedMulti  = EnemySpeedMulti;
            entity.HitPoints   = 1;
            return entity;
        }

        public EnemyEntity GetStageTwoEnemyEntity()
        {
            if (GameEngine.GameStage < 2)
                return GetRandomEnemyEntity();
            EnemyEntity entity  = new(GameArea, GetEnemyEntityRectangle(), Resources.coolPickle);
            entity.CanShoot     = true;
            entity.BaseSpeed    = Random.Next(200, 400) / 1000.0;
            entity.SpeedMulti   = EnemySpeedMulti;
            entity.HitPoints    = 2;
            entity.PointsOnKill = 300;
            return entity;
        }

        public EnemyEntity GetStageThreeEnemyEntity()
        {
            if (GameEngine.GameStage < 3)
                return GetRandomEnemyEntity();
            EnemyEntity entity  = new(GameArea, GetEnemyEntityRectangle(), Resources.coolestPickle);
            entity.CanShoot     = true;
            entity.BaseSpeed    = Random.Next(200, 300) / 1000.0;
            entity.SpeedMulti   = EnemySpeedMulti;
            entity.HitPoints    = 3;
            entity.PointsOnKill = 600;
            entity.MoveUp       = Random.Next(0, 1) == 1;
            entity.MoveDown     = ! entity.MoveUp;
            return entity;
        }

        public BossEntity GetBossEntity()
        {
            int x = (int)(GameArea.Width - GameArea.Width * 0.2);
            int y = (int)(GameArea.Height / 2 - GameArea.Width * 0.1);
            int w = (int)(GameArea.Width * 0.2);
            int h = (int)(GameArea.Width * 0.1);
            BossEntity entity = new(GameArea, new Rectangle(x, y, w, h));
            entity.MoveUp     = Random.Next(0, 1) == 1;
            entity.MoveDown   = !entity.MoveUp;
            return entity;
        }

        #region Utility Functions

        /// <summary>
        /// Store Resized/Modified sprite image in entity object
        /// </summary>
        public void CacheResizedEntitySprite(Image image, string key)
        {
            if (!EntityDataList.ContainsKey(key))
                return;

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
                return false;
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < (EntityDataList["player"] as PlayerEntity).DamageableCooldownTimer;
        }

        /// <summary>
        /// Return boss health percent eg. 70 - Or 0 if boss not spawned
        /// </summary>
        public int GetBossHealthPercent()
        {
            if (!EntityDataList.ContainsKey("boss"))
                return 0;
            return (int)((double)EntityDataList["boss"].HitPoints / BossEntity.MaxHP * 100);
        }

        #endregion
    }
}

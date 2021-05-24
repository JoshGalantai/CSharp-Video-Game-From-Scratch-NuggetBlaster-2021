using NuggetBlaster.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using NuggetBlaster.Properties;
using System.Linq;
using System.IO;
using System.Media;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        public           Rectangle   GameArea;
        private readonly GameForm    GameUI;
        private          SoundPlayer MusicPlayer;
        private readonly Random      Random = new();
        private readonly IDictionary<string, Entity> EntityDataList   = new Dictionary<string, Entity>();
        private readonly IDictionary<string, string> SoundEffectsList = new Dictionary<string, string>();

        // Engine Config
        public  const int Fps                   = 60;
        private const int MaxMSCatchUpPerTick   = 40;
        private const int MaxMSFallBehindCutoff = 1000;

        // Engine Vars
        public  int  TicksCurrent;
        public  int  TicksToProcess;
        public  bool IsRunning;
        private int  TicksTotal;
        private long MSStartTime;


        // Game Config
        public  const int  MaxPlayerHP              = 5;
        private const int  Stage2StartMS            = 60000;
        private const int  Stage3StartMS            = 120000;
        private const int  Stage4StartMS            = 180000;
        private const int  EnemySpawnCooldownMS     = 300;
        private const int  PowerUpSpawnInterval     = 7500;
        private const int  HeartPointsSpawnInterval = 10000;

        // Game Vars
        public  int    GameStage;
        public  int    Score;
        private long   EnemySpawnCooldownTimer;
        private int    NextPowerUpSpawnPoints;
        private int    NextHeartSpawnPoints;
        private double EnemySpeedMulti;
        private int    MaxEnemies;
        private int    EntityIterator;
        private int    EnemyCount;
        private bool   SpawnBoss;

        public Engine(GameForm gameUI) {
            GameUI    = gameUI;
            IsRunning = false;

            MusicPlayer = new(Resources.title);
            MusicPlayer.PlayLooping();

            // Async audio player needed for SFX cannot read from resource - Copy to local
            SoundEffectsList["boom"]    = GetResourceAsLocal(Resources.boom,    "boom");
            SoundEffectsList["shoot"]   = GetResourceAsLocal(Resources.shoot,   "shoot");
            SoundEffectsList["heal"]    = GetResourceAsLocal(Resources.heal,    "heal");
            SoundEffectsList["oof"]     = GetResourceAsLocal(Resources.oof,     "oof");
            SoundEffectsList["upgrade"] = GetResourceAsLocal(Resources.upgrade, "upgrade");
        }

        /***********************************************************************
         * START - Game State Interaction Methods                              *
         ***********************************************************************/

        public void StartGame()
        {
            GameArea    = GameUI.GetGameAreaAsRectangle();
            IsRunning   = true;
            MSStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            SpawnBoss = false;
            Score     = GameStage = TicksTotal = TicksCurrent = 0;

            NextHeartSpawnPoints   = HeartPointsSpawnInterval;
            NextPowerUpSpawnPoints = PowerUpSpawnInterval;

            if (!EntityDataList.ContainsKey("player"))
                EntityDataList["player"] = new PlayerEntity(GameArea);
            EntityDataList["player"].HitPoints = MaxPlayerHP;
        }

        public void GameOver()
        {
            IsRunning = false;
            ClearEntities();

            MusicPlayer = new(Resources.title);
            MusicPlayer.PlayLooping();
        }

        public void ProcessGameTick()
        {
            CalculateTicksToProcess();
            
            if (IsRunning)
            {
                CheckGameState();
                ProcessGameStage();
                ProcessEntityMovement();
                ProcessEntityCollissions();
                ProcessEntityCreation();
                ProcessProjectileCreation();
                CheckGameState();
            }
        }

        public void CheckGameState()
        {
            if (!IsRunning || !EntityDataList.ContainsKey("player"))
                GameOver();
        }

        public void ProcessGameStage()
        {
            double msPerTick = 1000.0 / Fps;
            if (TicksTotal * msPerTick < Stage2StartMS && GameStage != 1)
            {
                GameStage       = 1;
                MaxEnemies      = 6;
                EnemySpeedMulti = 1.0;

                MusicPlayer = new(Resources.stageOne);
                MusicPlayer.PlayLooping();
            }
            else if (TicksTotal * msPerTick > Stage2StartMS && GameStage == 1)
            {
                GameStage       = 2;
                MaxEnemies      = 7;
                EnemySpeedMulti = 1.1;

                MusicPlayer = new(Resources.stageTwo);
                MusicPlayer.PlayLooping();

            }
            else if (TicksTotal * msPerTick > Stage3StartMS && GameStage == 2)
            {
                GameStage       = 3;
                MaxEnemies      = 7;
                EnemySpeedMulti = 1.2;

                MusicPlayer = new(Resources.StageThree);
                MusicPlayer.PlayLooping();
            }
            else if (TicksTotal * msPerTick > Stage4StartMS && GameStage == 3)
            {
                GameStage       = 4;
                MaxEnemies      = 0;
                EnemySpeedMulti = 1.0;
                SpawnBoss       = true;

                MusicPlayer = new(Resources.bossStage);
                MusicPlayer.PlayLooping();
            }
            else if (GameStage == 4 && GetBossHealthPercent() == 0)
            {
                GameStage   = 5;
                MaxEnemies  = 7;

                MusicPlayer = new(Resources.postBoss);
                MusicPlayer.PlayLooping();
            }
            else if (GameStage == 5)
            {
                EnemySpeedMulti = 1.0 + Score * 0.000005; // Every 20k points + 0.1 speed multi
            }
        }

        /***********************************************************************
         * END - Game State Interaction Methods                                *
         ***********************************************************************/

        /***********************************************************************
         * START - Entity Interaction Methods                                  *
         ***********************************************************************/

        public void ProcessEntityCreation()
        {
            if (!IsRunning)
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
            if (Score > NextHeartSpawnPoints)
            {
                BuffEntity buff = new(GameArea, Random);
                NextHeartSpawnPoints += HeartPointsSpawnInterval;
                AddEntity(buff, "buff-");
            }
            if (Score > NextPowerUpSpawnPoints)
            {
                BuffShootEntity buff = new(GameArea, Random);
                NextPowerUpSpawnPoints += PowerUpSpawnInterval;
                AddEntity(buff, "buff-");
            }
        }
 
        public void ProcessEntityMovement()
        {
            if (!IsRunning)
                return;
            foreach (string key in EntityDataList.Keys.ToArray())
            {
                EntityDataList[key].CalculateMovement(TicksToProcess);
                if (!RectangleOverlaps(EntityDataList[key].SpriteRectangle, GameArea))
                    DeleteEntity(key);
            }
        }

        public void ProcessProjectileCreation()
        {
            if (!IsRunning)
                return;
            string[] entityKeys = EntityDataList.Keys.ToArray();
            foreach (string key in entityKeys)
            {
                foreach (ProjectileEntity entity in EntityDataList[key].Shoot()) {
                    AddEntity(entity, "proj-");
                }
            }
            if (EntityDataList.Count > entityKeys.Length)
                PlaySoundAsync(SoundEffectsList["shoot"]);
        }

        public void ProcessEntityCollissions()
        {
            if (!IsRunning)
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
                    if (RectangleOverlaps(EntityDataList[targetKey].SpriteRectangle, EntityDataList[comparisonKey].SpriteRectangle))
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
                                PlaySoundAsync(SoundEffectsList["heal"]);
                            if (EntityDataList[comparisonKey].GetType() == typeof(BuffShootEntity))
                                PlaySoundAsync(SoundEffectsList["upgrade"]);

                            DeleteEntity(comparisonKey, true);
                        }
                    }
                }
            }
            if (GetPlayerHP() < playerHPStart)
                PlaySoundAsync(SoundEffectsList["oof"]);
            else if (EnemyCount < enemyCountStart)
                PlaySoundAsync(SoundEffectsList["boom"]);
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
                IsRunning = false;
            if (EntityDataList[id].GetType() == typeof(EnemyEntity))
                EnemyCount--;
            if (addPoints)
                Score += EntityDataList[id].PointsOnKill;
            EntityDataList.Remove(id);
        }

        public EnemyEntity GetRandomEnemyEntity()
        {
            int enemySeed = Random.Next(0, 100);
            if (enemySeed > 80)
                return GetStage3EnemyEntity();
            else if (enemySeed > 40)
                return GetStage2EnemyEntity();
            else
                return GetStage1EnemyEntity();
        }

        public EnemyEntity GetStage1EnemyEntity()
        {
            EnemyEntity entity = new(GameArea, new Rectangle(GameArea.Width, Random.Next(10, GameArea.Height-(int)(GameArea.Width * 0.1)), (int)(GameArea.Width*0.1), (int)(GameArea.Width*0.1)), Resources.pickle);
            entity.BaseSpeed   = Random.Next(400, 600)/1000.0;
            entity.SpeedMulti  = EnemySpeedMulti;
            entity.HitPoints   = 1;
            return entity;
        }

        public EnemyEntity GetStage2EnemyEntity()
        {
            if (GameStage < 2)
                return GetRandomEnemyEntity();
            EnemyEntity entity  = new(GameArea, new Rectangle(GameArea.Width, Random.Next(10, GameArea.Height - (int)(GameArea.Width * 0.1)), (int)(GameArea.Width * 0.1), (int)(GameArea.Width * 0.1)), Resources.coolPickle);
            entity.CanShoot     = true;
            entity.BaseSpeed    = Random.Next(200, 400) / 1000.0;
            entity.SpeedMulti   = EnemySpeedMulti;
            entity.HitPoints    = 2;
            entity.PointsOnKill = 300;
            return entity;
        }

        public EnemyEntity GetStage3EnemyEntity()
        {
            if (GameStage < 3)
                return GetRandomEnemyEntity();
            EnemyEntity entity  = new(GameArea, new Rectangle(GameArea.Width, Random.Next(10, GameArea.Height - (int)(GameArea.Width * 0.1)), (int)(GameArea.Width * 0.1), (int)(GameArea.Width * 0.1)), Resources.coolestPickle);
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
            BossEntity entity = new(GameArea, new Rectangle((int)(GameArea.Width-GameArea.Width * 0.2), (int)(GameArea.Height/2-GameArea.Width*0.1), (int)(GameArea.Width * 0.2), (int)(GameArea.Width * 0.1)));
            entity.MoveUp     = Random.Next(0, 1) == 1;
            entity.MoveDown   = !entity.MoveUp;
            return entity;
        }

        /***********************************************************************
         * END - Entity Interaction Methods                                    *
         ***********************************************************************/

        /***********************************************************************
         * START - UI Interaction Methods                                      *
         ***********************************************************************/

        public void GameKeyAction(string key, bool pressed)
        {
            if (EntityDataList.ContainsKey("player"))
            {
                if (key.ToLower() == "up")
                    EntityDataList["player"].MoveUp = pressed;
                if (key.ToLower() == "down")
                    EntityDataList["player"].MoveDown = pressed;
                if (key.ToLower() == "left")
                    EntityDataList["player"].MoveLeft = pressed;
                if (key.ToLower() == "right")
                    EntityDataList["player"].MoveRight = pressed;
                if (key.ToLower() == "space")
                    EntityDataList["player"].Spacebar = pressed;
            }
            else if (key.ToLower() == "return")
                StartGame();
        }

        public static void PlaySoundAsync(string path)
        {
            MediaPlayer MediaPlayer = new();
            MediaPlayer.Open(new Uri(path));
            MediaPlayer.Volume = 1;
            MediaPlayer.Play();
        }

        public void CacheResizedEntitySprite(Image image, string key)
        {
            if (!EntityDataList.ContainsKey(key))
                return;

            EntityDataList[key].SpriteCached = image;
        }

        /***********************************************************************
         * END - UI Interaction Methods                                        *
         ***********************************************************************/

        /***********************************************************************
         * START - Utility Methods                                             *
         ***********************************************************************/

        public void CalculateTicksToProcess()
        {
            TicksToProcess = 1;
            long   msElapsed      = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - MSStartTime;
            double msPerTick      = 1000.0 / Fps;
            double msBehind       = msElapsed - (TicksCurrent * msPerTick);
            if (msBehind > MaxMSFallBehindCutoff)
            {
                MSStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                TicksCurrent = 0;
            }
            else if (msBehind > msPerTick)
            {
                TicksToProcess = msBehind > MaxMSCatchUpPerTick ? (int)(MaxMSCatchUpPerTick / msPerTick) : (int)(msBehind / msPerTick);
            }
            TicksCurrent += TicksToProcess;
            TicksTotal += TicksToProcess;
        }

        public IDictionary<string, Rectangle> GetEntityRectangleList()
        {
            IDictionary<string, Rectangle> rectangleList = new Dictionary<string, Rectangle>();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                rectangleList[entity.Key] = entity.Value.SpriteRectangle;
            }
            return rectangleList;
        }

        public IDictionary<string, Image> GetEntitySpriteList(bool getOriginal = false)
        {
            IDictionary<string, Image> spriteList = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                spriteList[entity.Key] = getOriginal ? entity.Value.SpriteOriginal : entity.Value.SpriteCached;
            }
            return spriteList;
        }

        public static bool RectangleOverlaps(Rectangle rectangleOne, Rectangle rectangleTwo)
        {
            return rectangleOne.IntersectsWith(rectangleTwo);
        }

        public static double GetPPF(double PixelsPerSecond)
        {
            return PixelsPerSecond / Fps;
        }

        public static dynamic GetResourceAsLocal(dynamic file, string fileName)
        {
            string fullPath = AppDomain.CurrentDomain.BaseDirectory + @"/" + fileName + ".wav";
            if (!File.Exists(fullPath))
            {
                using var fileStream = File.Create(fullPath);
                file.CopyTo(fileStream);
            }
            return fullPath;
        }

        public int GetPlayerHP()
        {
            return (EntityDataList.ContainsKey("player")) ? EntityDataList["player"].HitPoints : 0;
        }

        public int GetBossHealthPercent()
        {
            if (!EntityDataList.ContainsKey("boss"))
                return 0;
            return (int)((double)EntityDataList["boss"].HitPoints / BossEntity.MaxHP * 100);
        }

        public dynamic GetBossRectangle()
        {
            if (!EntityDataList.ContainsKey("boss"))
                return 0;
            return (int)((double)EntityDataList["boss"].HitPoints / BossEntity.MaxHP * 100);
        }

        /***********************************************************************
         * END - Utility Methods                                               *
         ***********************************************************************/
    }
}
using NuggetBlaster.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using NuggetBlaster.Properties;
using System.Linq;
using System.IO;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        public const int  Fps                   = 30;
        public const int  MaxMSCatchUpPerTick   = 100;
        public const int  MaxMSFallBehindCutoff = 1000;
        public       long MSStartTime;
        public       long TickCount;
        public       int  TicksToProcess;

        private System.Media.SoundPlayer MusicPlayer;

        private readonly Random Random = new();

        private int EntityIterator = 0;
        private int EnemyCount     = 0;
        private int MaxEnemies     = 4;

        private long EnemySpawnCooldownTimer = 0;
        private int  EnemySpawnCooldownMS    = 400;

        private readonly GameForm GameUI;

        public readonly IDictionary<string, Entity> EntityDataList   = new Dictionary<string, Entity>();
        public readonly IDictionary<string, string> SoundEffectsList = new Dictionary<string, string>();

        public int  Score     = 0;
        public bool IsRunning = false;

        public Engine(GameForm gameUI) {
            GameUI      = gameUI;
            MusicPlayer = new(Resources.title);
            MusicPlayer.PlayLooping();

            // Async audio player needed for SFX cannot read from resource - Copy to local
            SoundEffectsList["boom"]  = GetResourceAsLocal(Resources.boom, "boom");
            SoundEffectsList["shoot"] = GetResourceAsLocal(Resources.shoot, "shoot");
        }

       /***********************************************************************
        * START - Game State Interaction Methods                              *
        ***********************************************************************/

        public void StartGame()
        {
            IsRunning = true;
            Score     = 0;
            ClearEntities();

            MusicPlayer = new(Resources.stageOne);
            MusicPlayer.PlayLooping();

            if ( !EntityDataList.ContainsKey("player"))
                EntityDataList["player"] = new PlayerEntity();

            MSStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TickCount   = 0;
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
                   TicksToProcess = 1;
            long   msElapsed      = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - MSStartTime;
            double msPerTick      = 1000 / Fps;
            double msBehind       = msElapsed - (TickCount * msPerTick);
            if (msBehind > MaxMSFallBehindCutoff)
            {
                MSStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                TickCount = 0;
            }
            else if (msBehind >= msPerTick * 2)
            {
                TicksToProcess = msBehind > MaxMSCatchUpPerTick ? (int)(MaxMSCatchUpPerTick / msPerTick) : (int)(msBehind / msPerTick);
            }
            TickCount += TicksToProcess;

            if (IsRunning)
            {
                CheckGameState();
                ProcessEntityMovement();
                ProcessEntityCollissions();
                ProcessEntityCreation();
                ProcessEntityProjectileCreation();
                CheckGameState();
            }
        }

        public void CheckGameState()
        {
            if (!IsRunning || !EntityDataList.ContainsKey("player"))
                GameOver();
        }

        /***********************************************************************
         * END - Game State Interaction Methods                                *
         ***********************************************************************/

        /***********************************************************************
         * START - Entity Interaction Methods                                  *
         ***********************************************************************/

        public void ProcessEntityRescale(double scaling)
        {
            if (!IsRunning)
                return;
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                entity.Value.SpriteRectangle = GameForm.ResizeRectangle(entity.Value.SpriteRectangle, scaling);
                entity.Value.SpriteResized   = GameForm.ResizeImage(entity.Value.SpriteOriginal, entity.Value.SpriteRectangle);
            }
        }

        public void ProcessEntityCreation()
        {
            if (!IsRunning)
                return;
            if (EnemyCount < MaxEnemies)
            {
                EnemyEntity enemy = new(new Rectangle(950, Random.Next(50, 450), 100, 100), Resources.pickle);
                enemy.CanShoot = true;
                enemy.MaxSpeed = Random.Next(enemy.BaseSpeed, enemy.BaseSpeed*2);
                AddEntity(enemy, "enemy-");
            }
        }
 
        // Process entity movement & Out of bounds handling
        public void ProcessEntityMovement()
        {
            if (!IsRunning)
                return;
            foreach (string key in EntityDataList.Keys.ToArray())
            {
                EntityDataList[key].CalculateMovement(GameUI.GetGameCanvasAsRectangle(), TicksToProcess);
                if (!RectangleOverlaps(EntityDataList[key].SpriteRectangle, GameUI.GetGameCanvasAsRectangle()))
                    DeleteEntity(key);
            }
        }

        public void ProcessEntityProjectileCreation()
        {
            if (!IsRunning)
                return;
            string[] entityKeys = EntityDataList.Keys.ToArray();
            foreach (string key in entityKeys)
            {
                if (EntityDataList[key].CheckCanShoot())
                    AddEntity(EntityDataList[key].Shoot(), "proj-");
            }
            if (EntityDataList.Count > entityKeys.Length)
                PlaySoundAsync(SoundEffectsList["shoot"]);
        }

        public void ProcessEntityCollissions()
        {
            if (!IsRunning)
                return;
            string[] entityKeys = EntityDataList.Keys.ToArray();
            foreach (string nonProjKey in entityKeys)
            {
                if (!EntityDataList.ContainsKey(nonProjKey)||EntityDataList[nonProjKey].GetType() == typeof(ProjectileEntity))
                    continue;
                foreach (string comparisonKey in entityKeys)
                {
                    if (comparisonKey == nonProjKey || !EntityDataList.ContainsKey(comparisonKey) || !EntityDataList.ContainsKey(nonProjKey))
                        continue;
                    if (EntityDataList[comparisonKey].GetType() == typeof(ProjectileEntity) && EntityDataList[comparisonKey].Team != EntityDataList[nonProjKey].Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjKey].SpriteRectangle, EntityDataList[comparisonKey].SpriteRectangle))
                        {
                            DeleteEntity(comparisonKey, true);

                            EntityDataList[nonProjKey].HitPoints--;
                            if (EntityDataList[nonProjKey].HitPoints < 1)
                                DeleteEntity(nonProjKey, true);
                        }
                    }
                    else if (EntityDataList[comparisonKey].GetType() != typeof(ProjectileEntity) && EntityDataList[comparisonKey].Team != EntityDataList[nonProjKey].Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjKey].SpriteRectangle, EntityDataList[comparisonKey].SpriteRectangle))
                        {
                            EntityDataList[nonProjKey].HitPoints--;
                            if (EntityDataList[nonProjKey].HitPoints < 1)
                                DeleteEntity(nonProjKey, true);

                            EntityDataList[comparisonKey].HitPoints--;
                            if (EntityDataList[comparisonKey].HitPoints < 1)
                                DeleteEntity(comparisonKey, true);
                        }
                    }
                }
            }
            if (EntityDataList.Count < entityKeys.Length)
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
            if (EntityDataList.ContainsKey(id) && EntityDataList[id].GetType() == typeof(PlayerEntity))
                IsRunning = false;
            if (EntityDataList.ContainsKey(id) && EntityDataList[id].GetType() == typeof(EnemyEntity))
                EnemyCount--;
            if (addPoints)
                Score += EntityDataList[id].PointsOnKill;
            EntityDataList.Remove(id);
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
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(path));
            mediaPlayer.Volume = 1;
            mediaPlayer.Play();
        }

       /***********************************************************************
        * END - UI Interaction Methods                                        *
        ***********************************************************************/

       /***********************************************************************
        * START - Utility Methods                                             *
        ***********************************************************************/

        public IDictionary<string, Rectangle> GetEntityRectangleList()
        {
            IDictionary<string, Rectangle> rectangleList = new Dictionary<string, Rectangle>();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                rectangleList[entity.Key] = entity.Value.SpriteRectangle;
            }
            return rectangleList;
        }

        public IDictionary<string, Image> GetEntitySpriteList()
        {
            IDictionary<string, Image> spriteList = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                spriteList[entity.Key] = entity.Value.SpriteResized;
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
            using (var fileStream = File.Create(fullPath))
            {
                    file.CopyTo(fileStream);
            }
            return fullPath;
        }

       /***********************************************************************
        * END - Utility Methods                                               *
        ***********************************************************************/
    }
}
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
        public const int Fps = 60;

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
        public bool isRunning = false;

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
            isRunning = true;
            Score     = 0;
            ClearEntities();

            MusicPlayer = new(Resources.stageOne);
            MusicPlayer.PlayLooping();

            if ( !EntityDataList.ContainsKey("player"))
                EntityDataList["player"] = new PlayerEntity();
        }

        public void GameOver()
        {
            isRunning = false;
            ClearEntities();

            MusicPlayer = new(Resources.title);
            MusicPlayer.PlayLooping();
        }

        public void ProcessGameTick()
        {
            CheckGameState();
            ProcessEntityOutOfBounds();
            ProcessEntityCollissions();
            ProcessEntityCreation();
            ProcessEntityProjectileCreation();
            CheckGameState();
        }

        public void CheckGameState()
        {
            if (!isRunning || !EntityDataList.ContainsKey("player"))
                GameOver();
        }

        /***********************************************************************
         * END - Game State Interaction Methods                                *
         ***********************************************************************/

        /***********************************************************************
         * START - Entity Interaction Methods                                  *
         ***********************************************************************/

        public void ProcessEntityCreation()
        {
            if (!isRunning)
                return;
            if (EnemyCount < MaxEnemies)
            {
                EnemyEntity enemy = new(new Rectangle(950, Random.Next(50, 450), 100, 100), Resources.pickle);
                enemy.CanShoot = true;
                enemy.MaxSpeed = Random.Next(400, 700);
                AddEntity(enemy, "enemy-");
            }
        }
 
        public void ProcessEntityOutOfBounds()
        {
            if (!isRunning)
                return;
            IDictionary<string, Entity> entityDataListClone = EntityDataList.ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, Entity> entity in entityDataListClone)
            {
                EntityDataList[entity.Key].CalculateMovement(GameUI.GetGameCanvasAsRectangle());
                if (!RectangleOverlaps(EntityDataList[entity.Key].SpriteRectangle, GameUI.GetGameCanvasAsRectangle()))
                    DeleteEntity(entity.Key);
            }
        }

        public void ProcessEntityProjectileCreation()
        {
            if (!isRunning)
                return;
            IDictionary<string, Entity> entityDataListClone = EntityDataList.ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, Entity> entity in entityDataListClone)
            {
                if (EntityDataList[entity.Key].CheckCanShoot())
                    AddEntity(EntityDataList[entity.Key].Shoot(), "proj-");
            }
            if (EntityDataList.Count > entityDataListClone.Count)
                PlaySoundAsync(SoundEffectsList["shoot"]);
        }

        public void ProcessEntityCollissions()
        {
            if (!isRunning)
                return;
            IDictionary<string, Entity> entityDataListClone = EntityDataList.ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, Entity> nonProjEntity in entityDataListClone)
            {
                if (nonProjEntity.Value.GetType() == typeof(ProjectileEntity) || !EntityDataList.ContainsKey(nonProjEntity.Key))
                    continue;
                foreach (KeyValuePair<string, Entity> comparisonEntity in entityDataListClone)
                {
                    if (comparisonEntity.Key == nonProjEntity.Key || !EntityDataList.ContainsKey(comparisonEntity.Key) || !EntityDataList.ContainsKey(nonProjEntity.Key))
                        continue;
                    if (comparisonEntity.Value.GetType() == typeof(ProjectileEntity) && comparisonEntity.Value.Team != nonProjEntity.Value.Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjEntity.Key].SpriteRectangle, EntityDataList[comparisonEntity.Key].SpriteRectangle))
                        {
                            DeleteEntity(comparisonEntity.Key, true);

                            EntityDataList[nonProjEntity.Key].HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                                DeleteEntity(nonProjEntity.Key, true);
                        }
                    }
                    else if (comparisonEntity.Value.GetType() != typeof(ProjectileEntity) && comparisonEntity.Value.Team != nonProjEntity.Value.Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjEntity.Key].SpriteRectangle, EntityDataList[comparisonEntity.Key].SpriteRectangle))
                        {
                            EntityDataList[nonProjEntity.Key].HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                                DeleteEntity(nonProjEntity.Key, true);

                            EntityDataList[comparisonEntity.Key].HitPoints--;
                            if (comparisonEntity.Value.HitPoints < 1)
                                DeleteEntity(comparisonEntity.Key, true);
                        }
                    }
                }
            }
            if (EntityDataList.Count < entityDataListClone.Count)
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
                isRunning = false;
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
            IDictionary<string, Image> rectangleList = new Dictionary<string, Image>();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                rectangleList[entity.Key] = entity.Value.Sprite;
            }
            return rectangleList;
        }

        public static bool RectangleOverlaps(Rectangle rectangleOne, Rectangle rectangleTwo)
        {
            return rectangleOne.IntersectsWith(rectangleTwo);
        }

        public static int GetPPF(double PixelsPerSecond)
        {
            return (int)Math.Round((double)(PixelsPerSecond / Fps), 0, MidpointRounding.ToEven);
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
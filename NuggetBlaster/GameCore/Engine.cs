using NuggetBlaster.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using NuggetBlaster.Properties;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        public const int Fps = 60;

        private readonly System.Media.SoundPlayer Title    = new(Resources.title);
        private readonly System.Media.SoundPlayer StageOne = new(Resources.stage1);

        public  readonly IDictionary<string, Entity> EntityDataList = new Dictionary<string, Entity>();
        private readonly IDictionary<string, Entity> TempEntityList = new Dictionary<string, Entity>();

        private readonly Random Random = new();

        private int EntityIterator = 0;
        private int EnemyCount     = 0;
        private int MaxEnemies     = 4;

        private readonly GameForm GameUI;

        public int  Score     = 0;
        public bool isRunning = false;

        public Engine(GameForm gameUI) {
            GameUI = gameUI;
            Title.Play();
        }

        public static int GetPPF(double PixelsPerSecond)
        {
            return (int) Math.Round((double)(PixelsPerSecond / Fps), 0, MidpointRounding.ToEven);
        }

        public void StartGame()
        {
            Score = 0;
            ClearEntities();
            StageOne.Play();
            isRunning = true;

            if ( !EntityDataList.ContainsKey("player"))
                EntityDataList["player"] = new PlayerEntity(new Rectangle(15, 200, 100, 50), Resources.Nugget);
        }

        public void GameOver()
        {
            Title.Play();
            ClearEntities();
            isRunning = false;
        }

        public void ProcessGameTick()
        {
            bool playShoot = false;
            bool playBoom = false;
            if (!EntityDataList.ContainsKey("player"))
            {
                GameOver();
                return;
            }

            if (EntityDataList["player"].Spacebar && EntityDataList["player"].CheckCanShoot())
            {
                Point location = new(EntityDataList["player"].SpriteRectangle.Right, EntityDataList["player"].SpriteRectangle.Top + (EntityDataList["player"].SpriteRectangle.Height / 2));
                ProjectileEntity projectile = EntityDataList["player"].Shoot(new Rectangle(location, new Size(30, 16)), EntityDataList["player"].Team == 1 ? Resources.AllyProjectile : Resources.EnemyProjectile);
                AddEntity(projectile, "proj-");
                playShoot = true;
            }

            List<string> deleteList = new();
            foreach (KeyValuePair<string, Entity> entity in EntityDataList)
            {
                entity.Value.CalculateMovement(GameUI.GetGameCanvasAsRectangle());
                if (!RectangleOverlaps(entity.Value.SpriteRectangle, GameUI.GetGameCanvasAsRectangle()))
                {
                    deleteList.Add(entity.Key);
                    continue;
                }
                if (EntityDataList[entity.Key].GetType() == typeof(EnemyEntity) && entity.Value.CheckCanShoot())
                {
                    Point location = new(entity.Value.SpriteRectangle.Left - 20, entity.Value.SpriteRectangle.Top + (entity.Value.SpriteRectangle.Height / 2));
                    ProjectileEntity projectile = entity.Value.Shoot(new Rectangle(location, new Size(30, 16)), entity.Value.Team == 1 ? Resources.AllyProjectile : Resources.EnemyProjectile);
                    EntityIterator++;
                    AddToTempEntityList(projectile, "proj-");
                    playShoot = true;
                }
            }
            foreach (string id in deleteList)
                DeleteEntity(id);
            deleteList.Clear();
            AddAllTempEntities();

            foreach (KeyValuePair<string, Entity> nonProjEntity in EntityDataList)
            {
                if (nonProjEntity.Value.GetType() == typeof(ProjectileEntity) || deleteList.Contains(nonProjEntity.Key))
                {
                    continue;
                }
                foreach (KeyValuePair<string, Entity> comparisonEntity in EntityDataList)
                {
                    if (comparisonEntity.Key == nonProjEntity.Key || deleteList.Contains(comparisonEntity.Key) || deleteList.Contains(nonProjEntity.Key))
                        continue;
                    if (comparisonEntity.Value.GetType() == typeof(ProjectileEntity) && comparisonEntity.Value.Team != nonProjEntity.Value.Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjEntity.Key].SpriteRectangle, EntityDataList[comparisonEntity.Key].SpriteRectangle))
                        {
                            deleteList.Add(comparisonEntity.Key);
                            nonProjEntity.Value.HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                            {
                                deleteList.Add(nonProjEntity.Key);
                                Score += nonProjEntity.Value.PointsOnKill;
                                playBoom = true;
                            }
                        }
                    }
                    else if (comparisonEntity.Value.GetType() != typeof(ProjectileEntity) && comparisonEntity.Value.Team != nonProjEntity.Value.Team)
                    {
                        if (RectangleOverlaps(EntityDataList[nonProjEntity.Key].SpriteRectangle, EntityDataList[comparisonEntity.Key].SpriteRectangle))
                        {
                            nonProjEntity.Value.HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                            {
                                deleteList.Add(nonProjEntity.Key);
                                Score += comparisonEntity.Value.PointsOnKill;
                                playBoom = true;
                            }

                            comparisonEntity.Value.HitPoints--;
                            if (comparisonEntity.Value.HitPoints < 1)
                            {
                                deleteList.Add(comparisonEntity.Key);
                                Score += comparisonEntity.Value.PointsOnKill;
                                playBoom = true;
                            }
                        }
                    }
                }
            }

            foreach (string id in deleteList)
                DeleteEntity(id);

            if (EnemyCount < MaxEnemies)
            {
                EnemyEntity enemy = new(new Rectangle(950, Random.Next(50, 450), 100, 100), Resources.PlainPickle);
                enemy.MaxSpeed = Random.Next(400, 700);
                AddEntity(enemy, "enemy-");
            }

            if (playShoot)
                PlaySoundAsync(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\shoot.wav");
            if (playBoom)
                PlaySoundAsync(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\Boom.wav");

            if (!EntityDataList.ContainsKey("player"))
                GameOver();
        }

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
            else if(key.ToLower() == "return")
            {
                StartGame();
            }
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
            EntityDataList[prefix + EntityIterator.ToString()] = Data;
            EntityIterator++;
            if (Data.GetType() == typeof(EnemyEntity))
                EnemyCount++;
        }

        public void AddToTempEntityList(Entity entity, string prefix = "")
        {
            TempEntityList[prefix + EntityIterator.ToString()] = entity;
            EntityIterator++;
            if (entity.GetType() == typeof(EnemyEntity))
                EnemyCount++;
        }

        public void AddAllTempEntities()
        {
            foreach (KeyValuePair<string, Entity> entity in TempEntityList)
            {
                EntityDataList[entity.Key] = entity.Value;
            }
            TempEntityList.Clear();
        }

        public void DeleteEntity(string id)
        {
            if (EntityDataList.ContainsKey(id) && EntityDataList[id].GetType() == typeof(EnemyEntity))
                EnemyCount--;
            EntityDataList.Remove(id);
        }

        public static void PlaySoundAsync(string path)
        {
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(path));
            mediaPlayer.Play();
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
    }
}
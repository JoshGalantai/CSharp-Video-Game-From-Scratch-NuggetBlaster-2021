using NuggetBlaster.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Forms;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        public const int Fps = 30;

        private readonly System.Media.SoundPlayer Title    = new(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\title.wav");
        private readonly System.Media.SoundPlayer StageOne = new(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\stage1.wav");

        private readonly IDictionary<string, PictureBox> EntityUIList       = new Dictionary<string, PictureBox>();
        private readonly IDictionary<string, Entity>     EntityDataList     = new Dictionary<string, Entity>();
        private readonly IDictionary<string, PictureBox> TempEntityUIList   = new Dictionary<string, PictureBox>();
        private readonly IDictionary<string, Entity>     TempEntityDataList = new Dictionary<string, Entity>();

        private readonly Random Random = new();

        private int EntityCounter = 0;
        private int EnemyCounter  = 0;
        

        private readonly GameForm Form;

        public Engine(GameForm GameForm) {
            Form = GameForm;
            Title.Play();
        }

        public static int GetPPF(double PixelsPerSecond)
        {
            return (int) Math.Round((double)(PixelsPerSecond / Fps), 0, MidpointRounding.ToEven);
        }

        public void StartGame()
        {
            ClearEntities();
            Form.StartGameTimer();
            StageOne.Play();

            if ( ! EntityUIList.ContainsKey("player"))
            {
                PictureBox pictureBox = Form.CreatePicturebox("player", new Point(15, 200), new Size(100, 50), Image.FromFile(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\Nugget.png"));

                EntityUIList["player"]   = pictureBox;
                EntityDataList["player"] = new PlayerEntity();
            }
        }

        public void GameOver()
        {
            Title.Play();
            ClearEntities();
            Form.StopGameTimer();
        }

        public void ProcessGameTick()
        {
            bool playShoot = false;
            bool playBoom  = false;
            if (EntityDataList["player"].Spacebar && EntityDataList["player"].CheckCanShoot())
            {
                ProjectileEntity projectile = EntityDataList["player"].Shoot();
                Point location = new(EntityUIList["player"].Right, EntityUIList["player"].Top + (EntityUIList["player"].Height / 2));
                System.Drawing.Color color = projectile.Team == 1 ? System.Drawing.Color.FromArgb(0, 255, 0) : System.Drawing.Color.FromArgb(255, 0, 0);
                PictureBox pictureBox = Form.CreatePicturebox(EntityCounter.ToString(), location, new Size(15, 5), color);
                AddEntity(pictureBox, projectile, "proj-");
                playShoot = true;
            }

            List<string> deleteList         = new();


            foreach (KeyValuePair<string, PictureBox> UIEntity in EntityUIList)
            {
                if (EntityDataList[UIEntity.Key].GetType() == typeof(EnemyEntity) && EntityDataList[UIEntity.Key].CheckCanShoot())
                {
                    ProjectileEntity projectile = EntityDataList[UIEntity.Key].Shoot();
                    Point location = new(EntityUIList[UIEntity.Key].Left, EntityUIList[UIEntity.Key].Top + (EntityUIList[UIEntity.Key].Height / 2));
                    System.Drawing.Color color = projectile.Team == 1 ? System.Drawing.Color.FromArgb(0, 255, 0) : System.Drawing.Color.FromArgb(255, 0, 0);
                    PictureBox pictureBox = Form.CreatePicturebox(EntityCounter.ToString(), location, new Size(15, 5), color);
                    EntityCounter++;
                    AddToTempEntityList(pictureBox, projectile, "proj-");
                    playShoot = true;
                }
                GameForm.SetPictureBoxLocation(UIEntity.Value, EntityDataList[UIEntity.Key].CalculateMovement(UIEntity.Value.Location));
                if ( ! Form.PictureBoxInBounds(UIEntity.Value))
                    deleteList.Add(UIEntity.Key);
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
                        if (Form.PictureBoxOverlaps(EntityUIList[nonProjEntity.Key], EntityUIList[comparisonEntity.Key]))
                        {
                            deleteList.Add(comparisonEntity.Key);
                            nonProjEntity.Value.HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                            {
                                deleteList.Add(nonProjEntity.Key);
                                playBoom = true;
                            }
                        }
                    }
                    else if (comparisonEntity.Value.GetType() != typeof(ProjectileEntity) && comparisonEntity.Value.Team != nonProjEntity.Value.Team)
                    {
                        if (Form.PictureBoxOverlaps(EntityUIList[nonProjEntity.Key], EntityUIList[comparisonEntity.Key]))
                        {
                            nonProjEntity.Value.HitPoints--;
                            if (nonProjEntity.Value.HitPoints < 1)
                                deleteList.Add(nonProjEntity.Key);

                            comparisonEntity.Value.HitPoints--;
                            if (comparisonEntity.Value.HitPoints < 1)
                                deleteList.Add(comparisonEntity.Key);

                            if (nonProjEntity.Value.HitPoints < 1 || comparisonEntity.Value.HitPoints < 1)
                                playBoom = true;
                        }
                    }
                }
            }

            foreach (string id in deleteList)
                DeleteEntity(id);

            if (EnemyCounter < 4)
            {
                EnemyEntity enemy = new();
                enemy.MaxSpeed = Random.Next(400, 700);
                PictureBox pictureBox = Form.CreatePicturebox(EntityCounter.ToString(), new Point(950, Random.Next(50, 450)), new Size(50, 50), Image.FromFile(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\Pickle.png"));
                AddEntity(pictureBox, enemy, "enemy-");
            }

            if (playShoot)
                PlaySoundAsync(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\shoot.wav");
            if (playBoom)
                PlaySoundAsync(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\Boom.wav");

            if (!EntityUIList.ContainsKey("player"))
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
            foreach (KeyValuePair<string, PictureBox> UIEntity in EntityUIList)
                DeleteEntity(UIEntity.Key);

            EntityDataList.Clear();
            EntityUIList.Clear();
            EnemyCounter  = 0;
            EntityCounter = 0;
        }

        public void AddEntity(PictureBox UI, Entity Data, string prefix = "")
        {
            EntityUIList[prefix + EntityCounter.ToString()] = UI;
            EntityDataList[prefix + EntityCounter.ToString()] = Data;
            EntityCounter++;
            if (Data.GetType() == typeof(EnemyEntity))
                EnemyCounter++;
        }

        public void AddToTempEntityList(PictureBox UI, Entity Data, string prefix = "")
        {
            TempEntityUIList[prefix + EntityCounter.ToString()] = UI;
            TempEntityDataList[prefix + EntityCounter.ToString()] = Data;
            EntityCounter++;
            if (Data.GetType() == typeof(EnemyEntity))
                EnemyCounter++;
        }

        public void AddAllTempEntities()
        {
            foreach (KeyValuePair<string, PictureBox> UIEntity in TempEntityUIList)
            {
                EntityUIList[UIEntity.Key]   = UIEntity.Value;
                EntityDataList[UIEntity.Key] = TempEntityDataList[UIEntity.Key];
            }
            TempEntityDataList.Clear();
            TempEntityUIList.Clear();
        }

        public void DeleteEntity(string id)
        {
            if (EntityDataList.ContainsKey(id) && EntityDataList[id].GetType() == typeof(EnemyEntity))
                EnemyCounter--;
            Form.DeletePicturebox(EntityUIList[id]);
            EntityUIList.Remove(id);
            EntityDataList.Remove(id);
        }

        public static void PlaySoundAsync(string path)
        {
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(path));
            mediaPlayer.Play();
        }
    }
}
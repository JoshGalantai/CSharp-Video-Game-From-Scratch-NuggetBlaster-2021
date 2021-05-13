using NuggetBlaster.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Threading;
using System.Windows.Forms;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        public const int Fps = 30;

        System.Media.SoundPlayer title = new System.Media.SoundPlayer(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\title.wav");
        System.Media.SoundPlayer stage1 = new System.Media.SoundPlayer(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\stage1.wav");

        private IDictionary<string, PictureBox> EntityUIList   = new Dictionary<string, PictureBox>();
        private IDictionary<string, Entity>     EntityDataList = new Dictionary<string, Entity>();

        private int entityCounter = 0;

        private GameForm form;

        public Engine(GameForm GameForm) {
            form = GameForm;

            title.Play();
        }

        public void InitGame()
        {

        }



        public static int GetPPF(Double PixelsPerSecond)
        {
            return (int) Math.Round((Double)(PixelsPerSecond / Fps), 0, MidpointRounding.ToEven);
        }

        public void StartGame()
        {
            entityCounter = 0;
            form.StartGameTimer();
            stage1.Play();

            if ( ! EntityUIList.ContainsKey("player"))
            {
                PictureBox pictureBox = form.CreatePicturebox("player", new Point(15, 200), new Size(228, 125), Image.FromFile(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\Nugget.png"));

                EntityUIList["player"]   = pictureBox;
                EntityDataList["player"] = new PlayerEntity();
            }
        }

        public void GameOver()
        {
            entityCounter = 0;
            title.Play();
            ClearEntities();
            form.StopGameTimer();
        }

        public void ProcessGameTick()
        {
            if (!EntityUIList.ContainsKey("player"))
            {
                GameOver();
                return;
            }

            if (EntityDataList["player"].Spacebar && EntityDataList["player"].CheckCanShoot())
            {
                ProjectileEntity projectile = EntityDataList["player"].Shoot();
                Point location = new(EntityUIList["player"].Right, EntityUIList["player"].Top + (EntityUIList["player"].Height / 2));
                System.Drawing.Color color = projectile.Team == 1 ? System.Drawing.Color.FromArgb(0, 255, 0) : System.Drawing.Color.FromArgb(255, 0, 0);
                PictureBox pictureBox = form.CreatePicturebox(entityCounter.ToString(), location, new Size(15, 5), color);
                AddEntity(pictureBox, projectile);
                PlaySoundAsync(@"C:\Users\Josh\source\repos\NuggetBlaster\GameFiles\shoot.wav");
            }

            List<string> deleteList = new();
            foreach (KeyValuePair<string, PictureBox> UIEntity in EntityUIList)
            {
                GameForm.SetPictureBoxLocation(UIEntity.Value, EntityDataList[UIEntity.Key].CalculateMovement(UIEntity.Value.Location));
                if (UIEntity.Value.Left < 0 || UIEntity.Value.Right > 960 || UIEntity.Value.Top < 0 || UIEntity.Value.Bottom > 540)
                {
                    deleteList.Add(UIEntity.Key);
                }
            }

            foreach (string id in deleteList)
            {
                DeleteEntity(id);
            }
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
        }

        public void AddEntity(PictureBox UI, Entity Data)
        {
            EntityUIList[entityCounter.ToString()] = UI;
            EntityDataList[entityCounter.ToString()] = Data;
            entityCounter++;
        }

        public void DeleteEntity(string id)
        {
            form.DeletePicturebox(EntityUIList[id]);
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

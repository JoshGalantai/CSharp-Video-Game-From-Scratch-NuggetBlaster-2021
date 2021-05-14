using System;
using System.Drawing;
using System.Windows.Forms;
using NuggetBlaster.GameCore;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        private readonly Engine GameEngine;

        public GameForm()
        {
            InitializeComponent();

            GameEngine = new Engine(this);           
        }

        public void StartGameTimer()
        {
            gameTimer.Interval = 1000 / Engine.Fps;
            gameTimer.Start();
            NuggetBlasterText.Hide();
            InsertCoinText.Hide();
            PressEnterText.Hide();
        }

        public void StopGameTimer()
        {
            gameTimer.Stop();
            NuggetBlasterText.Show();
            InsertCoinText.Show();
            PressEnterText.Show();
        }

        private void GameKeyDown(object sender, KeyEventArgs e)
        {
            GameEngine.GameKeyAction(e.KeyCode.ToString(), true);
        }

        private void GameKeyUp(object sender, KeyEventArgs e)
        {
            GameEngine.GameKeyAction(e.KeyCode.ToString(), false);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            GameEngine.ProcessGameTick();

            if (background.Left < 0 - Width)
                background.Left = 0;
            background.Left -= Engine.GetPPF(200);
        }

        public PictureBox CreatePicturebox(string name, Point location, Size size, Color color)
        {
            PictureBox pictureBox = new()
            {
                Location  = location,
                Name      = name,
                Size      = size,
                BackColor = color
            };
            Controls.Add(pictureBox);
            pictureBox.BringToFront();

            return pictureBox;
        }

        public PictureBox CreatePicturebox(string name, Point location, Size size, Image image)
        {
            PictureBox pictureBox = new()
            {
                Location = location,
                Name = name,
                Image = image,
                Size = size,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(22, 2, 104)               
            };
            Controls.Add(pictureBox);
            pictureBox.BringToFront();

            return pictureBox;
        }

        public void DeletePicturebox(PictureBox pictureBox)
        {
            Controls.Remove(pictureBox);
        }

        public static void SetPictureBoxLocation(PictureBox pictureBox, Point location)
        {
            pictureBox.Location = location;
        }

        public bool PictureBoxInBounds(PictureBox picturebox)
        {
            return ClientRectangle.IntersectsWith(picturebox.Bounds);
        }

        public bool PictureBoxOverlaps(PictureBox pictureboxOne, PictureBox pictureboxTwo)
        {
            return pictureboxOne.Bounds.IntersectsWith(pictureboxTwo.Bounds);
        }
    }
}
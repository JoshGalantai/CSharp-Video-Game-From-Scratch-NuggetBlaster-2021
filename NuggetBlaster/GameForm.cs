using System;
using System.Drawing;
using System.Windows.Forms;
using NuggetBlaster.GameCore;
using NuggetBlaster.Helpers;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        private readonly Engine     GameEngine;
        private readonly DrawHelper DrawHelper;

        public const bool Analytics    = false;
        public       long DrawMS       = 0;
        public       long ProcessingMS = 0;     

        public GameForm()
        {
            InitializeComponent();

            GameEngine = new Engine(this);
            DrawHelper = new DrawHelper(this);

            GameTimer.Interval = 1000/Engine.Fps;
            GameTimer.Start();

            DrawHelper.ResizeUI();
        }

        public Rectangle GetGameAreaAsRectangle()
        {
            return new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        }

        public PictureBox GetGameCanvas()
        {
            return GameCanvas;
        }

        public void SetPlayerIsTranslucent(bool playerIsTranslucent)
        {
            DrawHelper.PlayerIsTranslucent = playerIsTranslucent;
        }

        #region Events

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
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GameEngine.ProcessGameTick();
            ProcessingMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;

            GameCanvas.Invalidate();
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            DrawHelper.DrawBackground(e.Graphics, GameEngine);

            if (GameEngine.IsRunning)
            {
                DrawHelper.DrawPlayerHealth(e.Graphics, GameEngine);
                DrawHelper.DrawBossHealthBarBottom(e.Graphics, GameEngine);
                DrawHelper.DrawGameSprites(e.Graphics, GameEngine);
                DrawHelper.DrawLevelText(e.Graphics, GameEngine);
            }
            else
            {
                DrawHelper.DrawTitle(e.Graphics);
            }

            DrawHelper.DrawScore(e.Graphics, GameEngine);
            DrawHelper.DrawKeys(e.Graphics);

            DrawMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
        }

        private void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
                DrawHelper.ResizeUI();
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
                DrawHelper.ResizeUI();
        }

        #endregion
    }
}
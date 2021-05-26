using System;
using System.Drawing;
using System.Windows.Forms;
using NuggetBlaster.GameCore;
using NuggetBlaster.Helpers;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        private readonly Engine      _gameEngine;
        private readonly DrawManager _drawHelper;

        public const bool Analytics    = false;
        public       long DrawMS       = 0;
        public       long ProcessingMS = 0;     

        public GameForm()
        {
            InitializeComponent();

            _gameEngine = new Engine(this);
            _drawHelper = new DrawManager(this);

            GameTimer.Interval = 1000/Engine.Fps;
            GameTimer.Start();

            _drawHelper.ResizeUI();
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
            _drawHelper.PlayerIsTranslucent = playerIsTranslucent;
        }

        #region Events

        private void GameKeyDown(object sender, KeyEventArgs e)
        {
            _gameEngine.GameKeyAction(e.KeyCode.ToString(), true);
        }

        private void GameKeyUp(object sender, KeyEventArgs e)
        {
            _gameEngine.GameKeyAction(e.KeyCode.ToString(), false);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _gameEngine.ProcessGameTick();
            ProcessingMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;

            GameCanvas.Invalidate();
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _drawHelper.DrawBackground(e.Graphics, _gameEngine);

            if (_gameEngine.IsRunning)
            {
                _drawHelper.DrawPlayerHealth(e.Graphics, _gameEngine);
                _drawHelper.DrawBossHealthBarBottom(e.Graphics, _gameEngine);
                _drawHelper.DrawGameSprites(e.Graphics, _gameEngine);
                _drawHelper.DrawLevelText(e.Graphics, _gameEngine);
            }
            else
            {
                _drawHelper.DrawTitle(e.Graphics);
            }

            _drawHelper.DrawScore(e.Graphics, _gameEngine);
            _drawHelper.DrawKeys(e.Graphics);

            DrawMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
        }

        private void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
                _drawHelper.ResizeUI();
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
                _drawHelper.ResizeUI();
        }

        #endregion
    }
}
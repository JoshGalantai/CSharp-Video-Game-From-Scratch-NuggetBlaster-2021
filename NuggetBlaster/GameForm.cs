using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        private readonly Engine GameEngine;

        private readonly Image     Background      = Resources.background;
        private          Rectangle BackgroundRect;
        private readonly Image     Keys            = Resources.keysWhite;
        private          Rectangle KeysRect        = new(0, 0, 225, 75);

        private readonly int CanvasEdgePadding = 10;

        private readonly string NuggetBlasterLabelText = "NUGGET BLASTER";
        private readonly Font   NuggetBlasterLabelFont = new("Showcard Gothic", 72F, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Size   NuggetBlasterLabelSize = new(840, 120);
        private readonly string InsertCoinLabelText    = "INSERT COIN";
        private readonly Font   InsertCoinLabelFont    = new("Arial Narrow", 26.25F, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Size   InsertCoinLabelSize    = new(200, 34);
        private readonly string PressEnterLabelText    = "- Press Enter -";
        private readonly Font   PressEnterLabelFont    = new("Arial Narrow", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Size   PressEnterLabelSize    = new(126, 24);
        private          string ScoreLabelText = "";
        private readonly Font   ScoreLabelFont = new("Arial Narrow", 26.25F, FontStyle.Regular, GraphicsUnit.Point);

        public GameForm()
        {
            InitializeComponent();

            GameTimer.Interval = 1000 / Engine.Fps;
            GameEngine         = new Engine(this);
            BackgroundRect     = new Rectangle(0, 0, GameCanvas.Width * 2, GameCanvas.Height);
            KeysRect           = new Rectangle(GameCanvas.Width - KeysRect.Width - CanvasEdgePadding, GameCanvas.Height - KeysRect.Height - CanvasEdgePadding, KeysRect.Width, KeysRect.Height);

            GameTimer.Start();
        }

        public Rectangle GetGameCanvasAsRectangle()
        {
            return new Rectangle(GameCanvas.Location, GameCanvas.Size);
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
            if (GameEngine.isRunning)
                GameEngine.ProcessGameTick();

            if (BackgroundRect.X < 0 - GameCanvas.Width)
                BackgroundRect.X = 0;
            BackgroundRect.X -= Engine.GetPPF(GameCanvas.Width / 10);

            GameCanvas.Invalidate();
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Background, BackgroundRect);
            if (GameEngine.isRunning)
            {
                IDictionary<string, Image> sprites = GameEngine.GetEntitySpriteList();
                foreach (KeyValuePair<string, Rectangle> rectangle in GameEngine.GetEntityRectangleList())
                    e.Graphics.DrawImage(sprites[rectangle.Key], rectangle.Value);
            }
            else
            {
                e.Graphics.DrawImage(Keys, KeysRect);
                e.Graphics.DrawString(NuggetBlasterLabelText, NuggetBlasterLabelFont, new SolidBrush(Color.White), GameCanvas.Width / 2 - NuggetBlasterLabelSize.Width / 2, GameCanvas.Height / 2 - NuggetBlasterLabelSize.Height / 2, new StringFormat());
                e.Graphics.DrawString(InsertCoinLabelText, InsertCoinLabelFont, new SolidBrush(Color.White), GameCanvas.Width / 2 - InsertCoinLabelSize.Width / 2, GameCanvas.Height / 2 + (NuggetBlasterLabelSize.Height / 2) + (InsertCoinLabelSize.Height / 2), new StringFormat());
                e.Graphics.DrawString(PressEnterLabelText, PressEnterLabelFont, new SolidBrush(Color.White), GameCanvas.Width / 2 - PressEnterLabelSize.Width / 2, GameCanvas.Height / 2 + (NuggetBlasterLabelSize.Height / 2) + (InsertCoinLabelSize.Height) + PressEnterLabelSize.Height, new StringFormat());
            }
            ScoreLabelText = "Score: " + GameEngine.Score;
            e.Graphics.DrawString(ScoreLabelText, ScoreLabelFont, new SolidBrush(Color.White), CanvasEdgePadding, CanvasEdgePadding, new StringFormat());
        }
    }
}
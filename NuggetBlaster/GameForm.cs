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

        // Display settings
        private const double AspectRatio       = (double)16 / 9;
        private const int    CanvasEdgePadding = 10;

        // Display items
        private Font      HeaderFont = new("Arial Narrow", 1, FontStyle.Regular, GraphicsUnit.Pixel);
        private Image     Background = Resources.background;
        private Image     Keys       = Resources.keysWhite;
        private Image     Title      = Resources.nuggetBlasterTitle;
        private Image     Heart      = Resources.heart;
        private Image     EmptyHeart = Resources.emptyHeart;
        private Rectangle BackgroundRect;
        private Rectangle KeysRect;
        private Rectangle TitleRect;
        private Rectangle HeartRect;
        private Rectangle BossHPRect;

        // Game vars
        public bool PlayerIsTranslucent = false; // After taking damage player becomes translucent for a short period

        // Analytics
        private const bool Analytics    = false;
        private       long msDraw       = 0;
        private       long msProcessing = 0;     

        public GameForm()
        {
            InitializeComponent();

            GameEngine = new Engine(this);

            GameTimer.Interval = 1000/Engine.Fps;
            GameTimer.Start();

            ResizeUI();
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
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GameEngine.ProcessGameTick();
            msProcessing += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;

            GameCanvas.Invalidate();
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            BackgroundRect.X = BackgroundRect.X < 0 - GameCanvas.Width ? 0 :BackgroundRect.X;
            BackgroundRect.X -= (int)(Engine.GetPPF(BackgroundRect.Width / 20) * GameEngine.TicksToProcess);
            e.Graphics.DrawImage(Background, BackgroundRect);

            if (GameEngine.IsRunning)
            {
                // Draw player health meter
                for (int i = 1; i <= Engine.MaxPlayerHP; i++)
                {
                    Point heartLocation = HeartRect.Location;
                    heartLocation.X += HeartRect.Width * (i-1);
                    e.Graphics.DrawImage(i > GameEngine.GetPlayerHP() ? EmptyHeart : Heart, new Rectangle(heartLocation, HeartRect.Size));
                }

                // Draw main boss HP bar at bottom of screen
                int bossHealthPercent = GameEngine.GetBossHealthPercent();
                if (bossHealthPercent > 0)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Red), BossHPRect);
                    e.Graphics.FillRectangle(new SolidBrush(Color.Lime), new Rectangle(BossHPRect.Location, new Size(BossHPRect.Width * bossHealthPercent / 100, BossHPRect.Height)));
                }

                double UIScaling = (double)GameCanvas.Width / GameEngine.GameArea.Width;
                IDictionary<string, Image> resizedSprites  = GameEngine.GetEntitySpriteList();
                IDictionary<string, Image> originalSprites = GameEngine.GetEntitySpriteList(true);
                Rectangle resizedRectangle;
                foreach (KeyValuePair<string, Rectangle> rectangle in GameEngine.GetEntityRectangleList()) {
                    resizedRectangle = ResizeRectangle(rectangle.Value, UIScaling);
                    if (resizedRectangle.Width != resizedSprites[rectangle.Key].Width || PlayerIsTranslucent != GameEngine.GetPlayerIsInvulnerable())
                    {
                        resizedSprites[rectangle.Key] = ResizeImage(originalSprites[rectangle.Key], resizedRectangle);

                        // Make player appear "ghostly" if they are currently in invincibility frames
                        if (GameEngine.GetPlayerIsInvulnerable())
                            resizedSprites[rectangle.Key] = ToolStripRenderer.CreateDisabledImage(resizedSprites[rectangle.Key]);
                        GameEngine.CacheResizedEntitySprite(resizedSprites[rectangle.Key], rectangle.Key);
                        PlayerIsTranslucent = GameEngine.GetPlayerIsInvulnerable();
                    }
                    e.Graphics.DrawImage(resizedSprites[rectangle.Key], resizedRectangle);
                    if (rectangle.Key == "boss")
                    {
                        if (bossHealthPercent > 0)
                        {
                            Point location = new((int)(rectangle.Value.X + rectangle.Value.Width * 0.3), (int)(rectangle.Value.Y - rectangle.Value.Width * 0.15));
                            Size  size     = new((int)(rectangle.Value.Width * 0.4), (int)(rectangle.Value.Width * 0.03));
                            e.Graphics.FillRectangle(new SolidBrush(Color.Red), new Rectangle(location, size));
                            e.Graphics.FillRectangle(new SolidBrush(Color.Lime), new Rectangle(location, new Size(size.Width * bossHealthPercent / 100, size.Height)));
                        }
                    }
                }
            }
            else
            {
                e.Graphics.DrawImage(Title, TitleRect);
            }

            string analytics = Analytics ? " ticks: " + GameEngine.TicksCurrent.ToString() + " drawMs: " + msDraw + " processMs: " + msProcessing : "";
            e.Graphics.DrawString("Score: " + GameEngine.Score + analytics, HeaderFont, new SolidBrush(Color.White), CanvasEdgePadding, CanvasEdgePadding, new StringFormat());

            e.Graphics.DrawImage(Keys, KeysRect);
            msDraw += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
        }

        private void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
                ResizeUI();
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Height != 0 && ClientSize.Width != 0 && (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width))
            {
                if (Size.Width != Size.Height * AspectRatio)
                    Size = new Size(Size.Width, (int)(Size.Width / AspectRatio));
                ResizeUI();
            }
                
        }

        private void ResizeUI()
        {
            if (Size.Width != Size.Height * AspectRatio)
                Size = new Size(Size.Width, (int)(Size.Width / AspectRatio));

            double scaling    = ClientSize.Width / (double)GameCanvas.Width;
            GameCanvas.Height = ClientSize.Height;
            GameCanvas.Width  = ClientSize.Width;

            HeaderFont = new Font("Arial Narrow", GameCanvas.Height/15, FontStyle.Regular, GraphicsUnit.Pixel);
            BackgroundRect = new Rectangle((int)(BackgroundRect.X*scaling), 0, GameCanvas.Width * 2, GameCanvas.Height);
            KeysRect       = new Rectangle((int)(GameCanvas.Width - (GameCanvas.Width * 0.2)) - CanvasEdgePadding, (int)(GameCanvas.Height - (GameCanvas.Height * 0.1)) - CanvasEdgePadding, (int)(GameCanvas.Width * 0.2), (int)(GameCanvas.Height * 0.1));
            TitleRect      = new Rectangle((int)(GameCanvas.Width / 2 - GameCanvas.Width * 0.8 / 2), (int)(GameCanvas.Height / 2 - GameCanvas.Height * 0.2 / 2), (int)(GameCanvas.Width * 0.8), (int)(GameCanvas.Height * 0.2));
            HeartRect      = new Rectangle(CanvasEdgePadding, GameCanvas.Height/15+CanvasEdgePadding*2, (int)(GameCanvas.Width*0.053), (int)(GameCanvas.Width * 0.036));
            BossHPRect     = new Rectangle((int)(GameCanvas.Width * 0.1), (int)(GameCanvas.Height - (GameCanvas.Width * 0.07)) - CanvasEdgePadding, (int)(GameCanvas.Width * 0.8), (int)(GameCanvas.Height * 0.025));

            Background = ResizeImage(Resources.background, BackgroundRect);
            Keys       = ResizeImage(Resources.keysWhite, KeysRect);
            Title      = ResizeImage(Resources.nuggetBlasterTitle, TitleRect);
            Heart      = ResizeImage(Resources.heart, HeartRect);
            EmptyHeart = ResizeImage(Resources.emptyHeart, HeartRect);
        }

        public static Image ResizeImage(Image image, Rectangle rect)
        {
            return new Bitmap(image, rect.Size);
        }

        public static Rectangle ResizeRectangle(Rectangle rectangle, double scaling)
        {
            return new Rectangle((int)(rectangle.X*scaling), (int)(rectangle.Y*scaling), (int)(rectangle.Width*scaling), (int)(rectangle.Height*scaling));
        }

        public Rectangle GetGameAreaAsRectangle()
        {
            return new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        }
    }
}
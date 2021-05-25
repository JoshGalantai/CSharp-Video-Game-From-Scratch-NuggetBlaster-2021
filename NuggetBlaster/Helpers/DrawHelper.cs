using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NuggetBlaster.Helpers
{
    class DrawHelper
    {
        private const double AspectRatio       = (double)16 / 9;
        private const int    CanvasEdgePadding = 10;

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
        private Point     LevelLocation;

        /// <summary>
        /// After taking damage player becomes translucent for a short period 
        /// </summary>
        public bool PlayerIsTranslucent = false;

        private readonly GameForm GameUI;

        public DrawHelper(GameForm gameUI)
        {
            GameUI = gameUI;
        }

        #region Draw Helper Functions

        /// <summary>
        /// Background moves a percentage of screen each second 
        /// </summary>
        public void DrawBackground(Graphics g, Engine engine)
        {
            BackgroundRect.X = BackgroundRect.X < 0 - GameUI.GetGameCanvas().Width ? 0 : BackgroundRect.X;
            BackgroundRect.X -= (int)(Engine.ConvertPerSecondToPerFrame(BackgroundRect.Width*0.04) * engine.TicksToProcess);
            g.DrawImage(Background, BackgroundRect);
        }

        public void DrawPlayerHealth(Graphics g, Engine engine)
        {
            Point heartLocation = HeartRect.Location;
            for (int i = 1; i <= EntityManager.MaxPlayerHP; i++)
            {
                g.DrawImage(i > engine.EntityManager.GetPlayerHP() ? EmptyHeart : Heart, new Rectangle(heartLocation, HeartRect.Size));
                heartLocation.X += HeartRect.Width;
            }
        }

        public void DrawBossHealthBarBottom(Graphics g, Engine engine)
        {
            if (engine.EntityManager.GetBossHealthPercent() > 0)
            {
                g.FillRectangle(new SolidBrush(Color.Red), BossHPRect);
                g.FillRectangle(new SolidBrush(Color.Lime), new Rectangle(BossHPRect.Location, new Size(BossHPRect.Width * engine.EntityManager.GetBossHealthPercent() / 100, BossHPRect.Height)));
            }
        }

        public void DrawLevelText(Graphics g, Engine engine)
        {
            g.DrawString("LEVEL " + engine.GameStage, HeaderFont, new SolidBrush(Color.White), LevelLocation.X, LevelLocation.Y, new StringFormat());
        }

        public void DrawTitle(Graphics g)
        {
            g.DrawImage(Title, TitleRect);
        }

        public void DrawScore(Graphics g, Engine engine)
        {
            string analytics = GameForm.Analytics ? " ticks: " + engine.TicksCurrent.ToString() + " drawMs: " + GameUI.DrawMS + " processMs: " + GameUI.ProcessingMS : "";
            g.DrawString("SCORE: " + engine.Score + analytics, HeaderFont, new SolidBrush(Color.White), CanvasEdgePadding, CanvasEdgePadding, new StringFormat());
        }

        public void DrawKeys(Graphics g)
        {
            g.DrawImage(Keys, KeysRect);
        }

        /// <summary>
        /// The "game area" size is fixed in the engine - resizing only happens at the UI layer -
        /// As the game resolution is adjusted we don't need to mess with entity position / movement calculations
        /// </summary>
        public void DrawGameSprites(Graphics g, Engine engine)
        {
            double UIScaling = (double)GameUI.GetGameCanvas().Width / engine.GameArea.Width;

            IDictionary<string, Image> resizedSprites  = engine.EntityManager.GetEntitySpriteList();
            IDictionary<string, Image> originalSprites = engine.EntityManager.GetEntitySpriteList(true);
            foreach (KeyValuePair<string, Rectangle> rectangle in engine.EntityManager.GetEntityRectangleList())
            {
                Rectangle resizedRectangle = ResizeRectangle(rectangle.Value, UIScaling);

                // Determine if the sprite's cached image needs to be updated - Resizing a bitmap each draw would be very expensive
                if (resizedRectangle.Width != resizedSprites[rectangle.Key].Width || (rectangle.Key == "player" && PlayerIsTranslucent != engine.EntityManager.IsPlayerInvulnerable()))
                {
                    // Update sprite's image to match new game resolution
                    resizedSprites[rectangle.Key] = ResizeImage(originalSprites[rectangle.Key], resizedRectangle);

                    // Make player appear "ghostly" if they are currently in invincibility frames
                    if (engine.EntityManager.IsPlayerInvulnerable() && rectangle.Key == "player")
                        resizedSprites[rectangle.Key] = ToolStripRenderer.CreateDisabledImage(resizedSprites[rectangle.Key]);
                    PlayerIsTranslucent = engine.EntityManager.IsPlayerInvulnerable();

                    // Cache updated image to improve performance
                    engine.EntityManager.CacheResizedEntitySprite(resizedSprites[rectangle.Key], rectangle.Key);
                }
                g.DrawImage(resizedSprites[rectangle.Key], resizedRectangle);

                // Draw small HP bar above boss entities
                if (rectangle.Key == "boss" && engine.EntityManager.GetBossHealthPercent() > 0)
                {
                    Point location = new((int)(rectangle.Value.X + rectangle.Value.Width * 0.3), (int)(rectangle.Value.Y - rectangle.Value.Width * 0.15));
                    Size size = new((int)(rectangle.Value.Width * 0.4), (int)(rectangle.Value.Width * 0.03));
                    g.FillRectangle(new SolidBrush(Color.Red), new Rectangle(location, size));
                    g.FillRectangle(new SolidBrush(Color.Lime), new Rectangle(location, new Size(size.Width * engine.EntityManager.GetBossHealthPercent() / 100, size.Height)));
                }
            }
        }

        /// <summary>
        /// Resize and "cache" all UI elements when game resolution changes - Resizing images on each draw phase would be very expensive 
        /// </summary>
        public void ResizeUI()
        {
            PictureBox gameCanvas = GameUI.GetGameCanvas();

            if (GameUI.Size.Width != GameUI.Size.Height * AspectRatio)
                GameUI.Size = new Size(GameUI.Size.Width, (int)(GameUI.Size.Width / AspectRatio));

            double scaling    = GameUI.ClientSize.Width / (double)gameCanvas.Width;
            gameCanvas.Height = GameUI.ClientSize.Height;
            gameCanvas.Width  = GameUI.ClientSize.Width;

            int x = (int)(BackgroundRect.X * scaling);
            int y = 0;
            int w = gameCanvas.Width * 2;
            int h = gameCanvas.Height;
            BackgroundRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width - (gameCanvas.Width * 0.2)) - CanvasEdgePadding;
            y = (int)(gameCanvas.Height - (gameCanvas.Height * 0.1)) - CanvasEdgePadding;
            w = (int)(gameCanvas.Width * 0.2);
            h = (int)(gameCanvas.Height * 0.1);
            KeysRect = new Rectangle(x, y, w, h);
            
            x = (int)(gameCanvas.Width / 2 - gameCanvas.Width * 0.8 / 2);
            y = (int)(gameCanvas.Height / 2 - gameCanvas.Height * 0.2 / 2);
            w = (int)(gameCanvas.Width * 0.8);
            h = (int)(gameCanvas.Height * 0.2);
            TitleRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width * 0.1);
            y = (int)(gameCanvas.Height - (gameCanvas.Width * 0.07)) - CanvasEdgePadding;
            w = (int)(gameCanvas.Width * 0.8);
            h = (int)(gameCanvas.Height * 0.025);
            BossHPRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width - (gameCanvas.Width * 0.13)) - CanvasEdgePadding;
            y = CanvasEdgePadding;
            LevelLocation = new Point(x, y);

            x = (int)(LevelLocation.X - (gameCanvas.Width * 0.05 * EntityManager.MaxPlayerHP) - CanvasEdgePadding);
            y = LevelLocation.Y;
            w = (int)(gameCanvas.Width * 0.05);
            h = (int)(gameCanvas.Width * 0.04);
            HeartRect = new Rectangle(x, y, w, h);

            h = (int)(gameCanvas.Height * 0.067);
            HeaderFont = new Font("Arial Narrow", h, FontStyle.Regular, GraphicsUnit.Pixel);

            Background = ResizeImage(Resources.background, BackgroundRect);
            Keys       = ResizeImage(Resources.keysWhite, KeysRect);
            Title      = ResizeImage(Resources.nuggetBlasterTitle, TitleRect);
            Heart      = ResizeImage(Resources.heart, HeartRect);
            EmptyHeart = ResizeImage(Resources.emptyHeart, HeartRect);
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Return input image resized to rectangle
        /// </summary>
        public static Image ResizeImage(Image image, Rectangle rect)
        {
            return new Bitmap(image, rect.Size);
        }

        /// <summary>
        /// Resize rectangle position and size according to resolution scaling
        /// </summary>
        public static Rectangle ResizeRectangle(Rectangle rectangle, double scaling)
        {
            return new Rectangle((int)(rectangle.X * scaling), (int)(rectangle.Y * scaling), (int)(rectangle.Width * scaling), (int)(rectangle.Height * scaling));
        }

        #endregion
    }
}
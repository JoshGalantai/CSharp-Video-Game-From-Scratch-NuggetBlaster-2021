using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NuggetBlaster.Helpers
{
    class DrawManager
    {
        private const double _aspectRatio       = (double)16 / 9;
        private const int    _canvasEdgePadding = 10;

        private Font      _headerFont = new("Arial Narrow", 1, FontStyle.Regular, GraphicsUnit.Pixel);
        private Image     _background = Resources.background;
        private Image     _keys       = Resources.keysWhite;
        private Image     _title      = Resources.nuggetBlasterTitle;
        private Image     _heart      = Resources.heart;
        private Image     _emptyHeart = Resources.emptyHeart;
        private Rectangle _backgroundRect;
        private Rectangle _keysRect;
        private Rectangle _titleRect;
        private Rectangle _heartRect;
        private Rectangle _bossHPRect;
        private Point     _levelLocation;

        /// <summary>
        /// After taking damage player becomes translucent for a short period 
        /// </summary>
        public bool PlayerIsTranslucent = false;

        private readonly GameForm GameUI;

        public DrawManager(GameForm gameUI)
        {
            GameUI = gameUI;
        }

        #region Draw Helper Functions

        /// <summary>
        /// Background moves a percentage of screen each second 
        /// </summary>
        public void DrawBackground(Graphics g, Engine engine)
        {
            _backgroundRect.X = _backgroundRect.X < 0 - GameUI.GetGameCanvas().Width ? 0 : _backgroundRect.X;
            _backgroundRect.X -= (int)(Engine.ConvertPerSecondToPerFrame(_backgroundRect.Width*0.04) * engine.TicksToProcess);
            g.DrawImage(_background, _backgroundRect);
        }

        public void DrawPlayerHealth(Graphics g, Engine engine)
        {
            Point heartLocation = _heartRect.Location;
            for (int i = 1; i <= EntityManager.MaxPlayerHP; i++)
            {
                g.DrawImage(i > engine.EntityManager.GetPlayerHP() ? _emptyHeart : _heart, new Rectangle(heartLocation, _heartRect.Size));
                heartLocation.X += _heartRect.Width;
            }
        }

        public void DrawBossHealthBarBottom(Graphics g, Engine engine)
        {
            if (engine.EntityManager.GetBossHealthPercent() > 0)
            {
                g.FillRectangle(new SolidBrush(Color.Red), _bossHPRect);
                g.FillRectangle(new SolidBrush(Color.Lime), new Rectangle(_bossHPRect.Location, new Size(_bossHPRect.Width * engine.EntityManager.GetBossHealthPercent() / 100, _bossHPRect.Height)));
            }
        }

        public void DrawLevelText(Graphics g, Engine engine)
        {
            g.DrawString("LEVEL " + engine.GameStage, _headerFont, new SolidBrush(Color.White), _levelLocation.X, _levelLocation.Y, new StringFormat());
        }

        public void DrawTitle(Graphics g)
        {
            g.DrawImage(_title, _titleRect);
        }

        public void DrawScore(Graphics g, Engine engine)
        {
            string analytics = GameForm.Analytics ? " ticks: " + engine.TicksCurrent.ToString() + " drawMs: " + GameUI.DrawMS + " processMs: " + GameUI.ProcessingMS : "";
            g.DrawString("SCORE: " + engine.Score + analytics, _headerFont, new SolidBrush(Color.White), _canvasEdgePadding, _canvasEdgePadding, new StringFormat());
        }

        public void DrawKeys(Graphics g)
        {
            g.DrawImage(_keys, _keysRect);
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

            if (GameUI.Size.Width != GameUI.Size.Height * _aspectRatio)
                GameUI.Size = new Size(GameUI.Size.Width, (int)(GameUI.Size.Width / _aspectRatio));

            double scaling    = GameUI.ClientSize.Width / (double)gameCanvas.Width;
            gameCanvas.Height = GameUI.ClientSize.Height;
            gameCanvas.Width  = GameUI.ClientSize.Width;

            int x = (int)(_backgroundRect.X * scaling);
            int y = 0;
            int w = gameCanvas.Width * 2;
            int h = gameCanvas.Height;
            _backgroundRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width - (gameCanvas.Width * 0.2)) - _canvasEdgePadding;
            y = (int)(gameCanvas.Height - (gameCanvas.Height * 0.1)) - _canvasEdgePadding;
            w = (int)(gameCanvas.Width * 0.2);
            h = (int)(gameCanvas.Height * 0.1);
            _keysRect = new Rectangle(x, y, w, h);
            
            x = (int)(gameCanvas.Width / 2 - gameCanvas.Width * 0.8 / 2);
            y = (int)(gameCanvas.Height / 2 - gameCanvas.Height * 0.2 / 2);
            w = (int)(gameCanvas.Width * 0.8);
            h = (int)(gameCanvas.Height * 0.2);
            _titleRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width * 0.1);
            y = (int)(gameCanvas.Height - (gameCanvas.Width * 0.07)) - _canvasEdgePadding;
            w = (int)(gameCanvas.Width * 0.8);
            h = (int)(gameCanvas.Height * 0.025);
            _bossHPRect = new Rectangle(x, y, w, h);

            x = (int)(gameCanvas.Width - (gameCanvas.Width * 0.13)) - _canvasEdgePadding;
            y = _canvasEdgePadding;
            _levelLocation = new Point(x, y);

            x = (int)(_levelLocation.X - (gameCanvas.Width * 0.05 * EntityManager.MaxPlayerHP) - _canvasEdgePadding);
            y = _levelLocation.Y;
            w = (int)(gameCanvas.Width * 0.05);
            h = (int)(gameCanvas.Width * 0.04);
            _heartRect = new Rectangle(x, y, w, h);

            h = (int)(gameCanvas.Height * 0.067);
            _headerFont = new Font("Arial Narrow", h, FontStyle.Regular, GraphicsUnit.Pixel);

            _background = ResizeImage(Resources.background, _backgroundRect);
            _keys       = ResizeImage(Resources.keysWhite, _keysRect);
            _title      = ResizeImage(Resources.nuggetBlasterTitle, _titleRect);
            _heart      = ResizeImage(Resources.heart, _heartRect);
            _emptyHeart = ResizeImage(Resources.emptyHeart, _heartRect);
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
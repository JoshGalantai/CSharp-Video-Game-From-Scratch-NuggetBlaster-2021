namespace NuggetBlaster.Helpers
{
  using System.Collections.Generic;
  using System.Drawing;
  using System.Windows.Forms;
  using GameCore;
  using Properties;
  using Entities;

  class DrawManager
  {
    // Enable for additional logging during gameplay
    private const bool _analytics = false;

    // Display Settings
    private const double _aspectRatio       = 1.77778; // 16:9
    private const int    _canvasEdgePadding = 10;

    // Text Information
    private Font  _headerFont = new("Arial Narrow", 1, FontStyle.Regular, GraphicsUnit.Pixel);
    private Image _title      = Resources.nuggetBlasterTitle;

    // HUD
    private Image     _keys       = Resources.keysWhite;
    private Image     _heart      = Resources.heart;
    private Image     _emptyHeart = Resources.emptyHeart;
    private Rectangle _keysRect;
    private Rectangle _titleRect;
    private Rectangle _heartRect;
    private Rectangle _bossHPRect;
    private Point     _levelLocation;

    // Background
    private Image     _background = Resources.background;
    private Image     _bgLayerBot = Resources.bgLayerBot;
    private Image     _bgLayerMid = Resources.bgLayerMid;
    private Image     _bgLayerTop = Resources.bgLayerTop;
    private Rectangle _backgroundRect;
    private Rectangle _bgLayerBotRect;
    private Rectangle _bgLayerMidRect;
    private Rectangle _bgLayerTopRect;

    /// <summary>
    /// Entity Images (Rectangles are stored on entity instances) - entities of a given type always appear the same, instead
    /// of storing duplicate images for each entity, just re-use the same images to save memory and cpu time for resizing.
    /// </summary>
    public readonly IDictionary<string, Image> EntitySprites = new Dictionary<string, Image>();

    /// <summary>
    /// After taking damage player becomes translucent for a short period 
    /// </summary>
    public bool PlayerIsTranslucent = false;

    private readonly GameForm _gameUI;

    public DrawManager(GameForm gameUI)
    {
      _gameUI = gameUI;

      EntitySprites[EntitySpriteType.ProjectileAlly.ToString()]      = Resources.allyProjectile;
      EntitySprites[EntitySpriteType.ProjectileAllySuper.ToString()] = Resources.allyProjectileSuper;
      EntitySprites[EntitySpriteType.ProjectileEnemy.ToString()]     = Resources.enemyProjectile;
      EntitySprites[EntitySpriteType.Boss.ToString()]                = Resources.bossPickle;
      EntitySprites[EntitySpriteType.BuffHeal.ToString()]            = Resources.buffHeart;
      EntitySprites[EntitySpriteType.BuffShoot.ToString()]           = Resources.buffShoot;
      EntitySprites[EntitySpriteType.EnemyLvlThree.ToString()]       = Resources.coolestPickle;
      EntitySprites[EntitySpriteType.EnemyLvlTwo.ToString()]         = Resources.coolPickle;
      EntitySprites[EntitySpriteType.EnemyLvlOne.ToString()]         = Resources.pickle;
      EntitySprites[EntitySpriteType.Player.ToString()]              = Resources.nugget;
    }

    #region Draw Helper Functions

    /// <summary>
    /// Appearance of infinite scroll is achieved by moving a horizontally tileable image a percentage of the screen width each frame.
    /// Rather than continuously creating images and adding them to the right, then destroying them when they move off screen, we use
    /// a single image that is twice the width of the of the canvas, with the tileable background repeated once. Once the image moves off
    /// the edge of the screen by 1 * canvas width, we reset the position to 0.
    /// 
    /// Example: ABCABC as the background image, and [   ] as the player view
    /// 1.    [ABC]ABC
    /// 2.   A[BCA]BC  - Left
    /// 3.  AB[CAB]C   - Left
    /// 4. ABC[ABC]    - Left
    /// 5.    [ABC]ABC - Reset + Repeat (Back to step 1)
    /// </summary>
    public void DrawBackground(Graphics g, Engine engine)
    {
      int canvasWidth = _gameUI.GetGameCanvas().Width;
      int baseBackgroundScrollPixels = (int)(Engine.ConvertPerSecondToPerFrame(_backgroundRect.Width * 0.02) * engine.TicksToProcess);

      // Background
      _backgroundRect.X = _backgroundRect.X < -canvasWidth ? 0 : _backgroundRect.X;
      _backgroundRect.X -= baseBackgroundScrollPixels;
      g.DrawImage(_background, _backgroundRect);

      // Parallax Bottom Layer
      _bgLayerBotRect.X = _bgLayerBotRect.X < -canvasWidth ? 0 : _bgLayerBotRect.X;
      _bgLayerBotRect.X -= baseBackgroundScrollPixels * 2;
      g.DrawImage(_bgLayerBot, _bgLayerBotRect);

      // Parallax Mid Layer
      _bgLayerMidRect.X = _bgLayerMidRect.X < -canvasWidth ? 0 : _bgLayerMidRect.X;
      _bgLayerMidRect.X -= baseBackgroundScrollPixels * 4;
      g.DrawImage(_bgLayerMid, _bgLayerMidRect);

      // Parallax Top Layer
      _bgLayerTopRect.X = _bgLayerTopRect.X < -canvasWidth ? 0 : _bgLayerTopRect.X;
      _bgLayerTopRect.X -= baseBackgroundScrollPixels * 8;
      g.DrawImage(_bgLayerTop, _bgLayerTopRect);
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
      string analytics = _analytics ? " ticks: " + engine.TicksCurrent.ToString() + " drawMs: " + _gameUI.DrawMS + " processMs: " + _gameUI.ProcessingMS : "";
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
      double UIScaling = (double)_gameUI.GetGameCanvas().Width / Engine.GameAreaWidth;

      IDictionary<string, Image> resizedSprites  = engine.EntityManager.GetEntitySpriteList();
      IDictionary<string, Image> originalSprites = engine.EntityManager.GetEntitySpriteList(true);
      foreach (KeyValuePair<string, Rectangle> rectangle in engine.EntityManager.GetEntityRectangleList())
      {
        Rectangle resizedRectangle = ResizeRectangle(rectangle.Value, UIScaling);

        // Determine if the sprite's cached image needs to be updated - Resizing a bitmap each draw would be very expensive
        if (resizedRectangle.Width != resizedSprites[rectangle.Key].Width || (rectangle.Key == "player" && this.PlayerIsTranslucent != engine.EntityManager.IsPlayerInvulnerable()))
        {
          // Update sprite's image to match new game resolution
          resizedSprites[rectangle.Key] = ResizeImage(originalSprites[rectangle.Key], resizedRectangle);

          // Make player appear "ghostly" if they are currently in invincibility frames
          if (engine.EntityManager.IsPlayerInvulnerable() && rectangle.Key == "player")
          {
            resizedSprites[rectangle.Key] = ToolStripRenderer.CreateDisabledImage(resizedSprites[rectangle.Key]);
          }
          this.PlayerIsTranslucent = engine.EntityManager.IsPlayerInvulnerable();

          // Cache updated image to improve performance
          engine.EntityManager.CacheResizedEntitySprite(resizedSprites[rectangle.Key], rectangle.Key);
        }
        g.DrawImage(resizedSprites[rectangle.Key], resizedRectangle);

        // Draw small HP bar above boss entities
        if (rectangle.Key == "boss" && engine.EntityManager.GetBossHealthPercent() > 0)
        {
          Point location = new((int)(rectangle.Value.X + rectangle.Value.Width * 0.3), (int)(rectangle.Value.Y - rectangle.Value.Width * 0.15));
          Size size      = new((int)(rectangle.Value.Width * 0.4), (int)(rectangle.Value.Width * 0.03));
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
      PictureBox gameCanvas = _gameUI.GetGameCanvas();

      if (_gameUI.Size.Width != _gameUI.Size.Height * _aspectRatio)
      {
        _gameUI.Size = new Size(_gameUI.Size.Width, (int)(_gameUI.Size.Width / _aspectRatio));
      }

      gameCanvas.Height = _gameUI.ClientSize.Height;
      gameCanvas.Width  = _gameUI.ClientSize.Width;

      // Background layers all have same dimensions with different x coordinates
      double scaling = _gameUI.ClientSize.Width / (double)gameCanvas.Width;
      int x = (int)(_backgroundRect.X * scaling);
      int y = 0;
      int w = gameCanvas.Width * 2;
      int h = gameCanvas.Height;
      _backgroundRect = new Rectangle(x, y, w, h);
      _bgLayerBotRect = new Rectangle((int)(_bgLayerBotRect.X * scaling), y, w, h);
      _bgLayerMidRect = new Rectangle((int)(_bgLayerMidRect.X * scaling), y, w, h);
      _bgLayerTopRect = new Rectangle((int)(_bgLayerTopRect.X * scaling), y, w, h);

      x = (int)(gameCanvas.Width * 0.8) - _canvasEdgePadding;
      y = (int)(gameCanvas.Height * 0.9) - _canvasEdgePadding;
      w = (int)(gameCanvas.Width * 0.2);
      h = (int)(gameCanvas.Height * 0.1);
      _keysRect = new Rectangle(x, y, w, h);

      x = (int)(gameCanvas.Width * 0.1);
      y = (int)(gameCanvas.Height * 0.4);
      w = (int)(gameCanvas.Width * 0.8);
      h = (int)(gameCanvas.Height * 0.2);
      _titleRect = new Rectangle(x, y, w, h);

      x = (int)(gameCanvas.Width * 0.1);
      y = (int)(gameCanvas.Height * 0.9) - _canvasEdgePadding;
      w = (int)(gameCanvas.Width * 0.8);
      h = (int)(gameCanvas.Height * 0.025);
      _bossHPRect = new Rectangle(x, y, w, h);

      x = (int)(gameCanvas.Width - (gameCanvas.Width * 0.05 * EntityManager.MaxPlayerHP) - _canvasEdgePadding);
      y = _canvasEdgePadding;
      w = (int)(gameCanvas.Width * 0.05);
      h = (int)(gameCanvas.Height * 0.07);
      _heartRect = new Rectangle(x, y, w, h);

      x = _canvasEdgePadding;
      y = (int)(gameCanvas.Height * 0.93) - _canvasEdgePadding;
      _levelLocation = new Point(x, y);

      int fontHeight = (int)(gameCanvas.Height * 0.07);
      _headerFont = new Font("Arial Narrow", fontHeight, FontStyle.Regular, GraphicsUnit.Pixel);

      _background = ResizeImage(Resources.background, _backgroundRect);
      _bgLayerBot = ResizeImage(Resources.bgLayerBot, _bgLayerBotRect);
      _bgLayerMid = ResizeImage(Resources.bgLayerMid, _bgLayerMidRect);
      _bgLayerTop = ResizeImage(Resources.bgLayerTop, _bgLayerTopRect);
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
namespace NuggetBlaster
{
  using System;
  using System.Windows.Forms;
  using NuggetBlaster.GameCore;
  using NuggetBlaster.Helpers;

  public partial class GameForm : Form
  {
    private readonly Engine gameEngine;
    private readonly DrawManager drawHelper;

    public GameForm()
    {
      this.InitializeComponent();

      this.gameEngine = new Engine(this);
      this.drawHelper = new DrawManager(this);

      gameTimer.Interval = 1000 / Engine.Fps;
      gameTimer.Start();

      this.drawHelper.ResizeUI();
    }

    public long DrawMS { get; set; } = 0;

    public long ProcessingMS { get; set; } = 0;

    public PictureBox GetGameCanvas()
    {
      return this.gameCanvas;
    }

    public void SetPlayerIsTranslucent(bool playerIsTranslucent)
    {
      this.drawHelper.PlayerIsTranslucent = playerIsTranslucent;
    }

    #region Events

    private void GameKeyDown(object sender, KeyEventArgs e)
    {
      this.gameEngine.GameKeyAction(e.KeyCode.ToString(), true);
    }

    private void GameKeyUp(object sender, KeyEventArgs e)
    {
      this.gameEngine.GameKeyAction(e.KeyCode.ToString(), false);
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
      long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      this.gameEngine.ProcessGameTick();
      this.ProcessingMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;

      gameCanvas.Invalidate();
    }

    private void GameCanvas_Paint(object sender, PaintEventArgs e)
    {
      long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      this.drawHelper.DrawBackground(e.Graphics, this.gameEngine);

      if (this.gameEngine.IsRunning)
      {
        this.drawHelper.DrawPlayerHealth(e.Graphics, this.gameEngine);
        this.drawHelper.DrawBossHealthBarBottom(e.Graphics, this.gameEngine);
        this.drawHelper.DrawGameSprites(e.Graphics, this.gameEngine);
        this.drawHelper.DrawLevelText(e.Graphics, this.gameEngine);
      }
      else
      {
        this.drawHelper.DrawTitle(e.Graphics);
      }

      this.drawHelper.DrawScore(e.Graphics, this.gameEngine);
      this.drawHelper.DrawKeys(e.Graphics);

      this.DrawMS += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
    }

    private void GameForm_ResizeEnd(object sender, EventArgs e)
    {
      if (ClientSize.Height != 0 && ClientSize.Width != 0 && (gameCanvas.Height != ClientSize.Height || gameCanvas.Width != ClientSize.Width))
      {
        this.drawHelper.ResizeUI();
      }
    }

    private void GameForm_Resize(object sender, EventArgs e)
    {
      if (ClientSize.Height != 0 && ClientSize.Width != 0 && (gameCanvas.Height != ClientSize.Height || gameCanvas.Width != ClientSize.Width))
      {
        this.drawHelper.ResizeUI();
      }
    }

    #endregion
  }
}
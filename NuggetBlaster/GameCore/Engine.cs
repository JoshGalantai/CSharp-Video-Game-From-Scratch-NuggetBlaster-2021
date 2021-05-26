using System;
using System.Drawing;
using System.Windows.Media;
using NuggetBlaster.Properties;
using System.IO;
using System.Media;

namespace NuggetBlaster.GameCore
{
    class Engine
    {
        private          SoundPlayer   _musicPlayer;
        private readonly GameForm      _gameUI;
        public           Rectangle     GameArea;
        public  readonly EntityManager EntityManager;

        // Engine Config
        public const int Fps                   = 60;
        public const int MaxMSCatchUpPerTick   = 40;
        public const int MaxMSFallBehindCutoff = 1000;

        // Game Config
        private const int _stage2StartMS = 60000;
        private const int _stage3StartMS = 120000;
        private const int _stage4StartMS = 180000;

        // Engine Vars
        private int  _ticksTotal;
        private long _msStartTime;
        public  int  TicksCurrent;
        public  int  TicksToProcess;
        public  bool IsRunning;

        // Game Vars
        public int GameStage;
        public int Score;

        public Engine(GameForm gameUI) {
            _gameUI   = gameUI;
            IsRunning = false;

            EntityManager = new EntityManager(this);

            _musicPlayer = new(Resources.title);
            _musicPlayer.PlayLooping();
        }

        #region Manage Game State

        public void StartGame()
        {
            IsRunning    = true;
            _msStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            Score = GameStage = _ticksTotal = TicksCurrent = 0;

            _gameUI.SetPlayerIsTranslucent(false);

            GameArea = _gameUI.GetGameAreaAsRectangle();
            EntityManager.Reset(GameArea);
        }

        public void GameOver()
        {
            IsRunning = false;
            EntityManager.ClearEntities();

            _musicPlayer = new(Resources.title);
            _musicPlayer.PlayLooping();
        }

        public void ProcessGameTick()
        {
            CalculateTicksToProcess();
            
            if (IsRunning)
            {
                CheckGameState();
                ProcessGameStage();
                EntityManager.ProcessEntityMovement();
                EntityManager.ProcessEntityCollisions();
                EntityManager.ProcessEntityCreation();
                EntityManager.ProcessProjectileCreation();
                CheckGameState();
            }
        }

        public void CheckGameState()
        {
            if (!IsRunning || !EntityManager.EntityDataList.ContainsKey("player"))
                GameOver();
        }

        /// <summary>
        /// Progress game stages
        /// </summary>
        public void ProcessGameStage()
        {
            double msPerTick = 1000.0 / Fps;
            if (_ticksTotal * msPerTick < _stage2StartMS && GameStage != 1)
                SetGameStageOne();
            else if (_ticksTotal * msPerTick > _stage2StartMS && GameStage == 1)
                SetGameStageTwo();
            else if (_ticksTotal * msPerTick > _stage3StartMS && GameStage == 2)
                SetGameStageThree();
            else if (_ticksTotal * msPerTick > _stage4StartMS && GameStage == 3)
                SetGameStageFour();
            else if (GameStage == 4 && EntityManager.GetBossHealthPercent() == 0)
                SetGameStageFive();
            else if (GameStage == 5)
                EntityManager.EnemySpeedMulti = 1.0 + Score * 0.000005; // Every 20k points + 0.1 speed multi
        }

        #endregion

        #region Input Handlers

        public void GameKeyAction(string key, bool pressed)
        {
            if (EntityManager.EntityDataList.ContainsKey("player"))
            {
                if (key.ToLower() == "up")
                    EntityManager.EntityDataList["player"].MoveUp = pressed;
                if (key.ToLower() == "down")
                    EntityManager.EntityDataList["player"].MoveDown = pressed;
                if (key.ToLower() == "left")
                    EntityManager.EntityDataList["player"].MoveLeft = pressed;
                if (key.ToLower() == "right")
                    EntityManager.EntityDataList["player"].MoveRight = pressed;
                if (key.ToLower() == "space")
                    EntityManager.EntityDataList["player"].Spacebar = pressed;
            }
            else if (key.ToLower() == "return")
                StartGame();
        }

        #endregion

        #region Set Game Stages

        public void SetGameStageOne()
        {
            GameStage                     = 1;
            EntityManager.MaxEnemies      = 6;
            EntityManager.EnemySpeedMulti = 1.0;

            _musicPlayer = new(Resources.stageOne);
            _musicPlayer.PlayLooping();
        }

        public void SetGameStageTwo()
        {
            GameStage                     = 2;
            EntityManager.MaxEnemies      = 7;
            EntityManager.EnemySpeedMulti = 1.1;

            _musicPlayer = new(Resources.stageTwo);
            _musicPlayer.PlayLooping();
        }

        public void SetGameStageThree()
        {
            GameStage                     = 3;
            EntityManager.MaxEnemies      = 7;
            EntityManager.EnemySpeedMulti = 1.2;

            _musicPlayer = new(Resources.StageThree);
            _musicPlayer.PlayLooping();
        }

        public void SetGameStageFour()
        {
            GameStage                     = 4;
            EntityManager.MaxEnemies      = 0;
            EntityManager.EnemySpeedMulti = 1.0;
            EntityManager.SpawnBoss       = true;

            _musicPlayer = new(Resources.bossStage);
            _musicPlayer.PlayLooping();
        }

        public void SetGameStageFive()
        {
            GameStage                     = 5;
            EntityManager.MaxEnemies      = 7;
            EntityManager.EnemySpeedMulti = 1.0;

            _musicPlayer = new(Resources.postBoss);
            _musicPlayer.PlayLooping();
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Play sound effect async - Use when multiple concurrent sounds are needed such as shooting or explosions
        /// Mediaplayer cannot read files from resource - Must access files from local directory - Resources are written to lcoal using utility
        /// </summary>
        public static void PlaySoundAsync(string path)
        {
            MediaPlayer MediaPlayer = new();
            MediaPlayer.Open(new Uri(path));
            MediaPlayer.Volume = 1;
            MediaPlayer.Play();
        }

        /// <summary>
        /// Determine how many "ticks" for current processing phase. If a few "ticks" are slow, we can
        /// try to "catch up" by handling extra ticks in subsequent process phases to smooth out
        /// gameplay. If we fall too far behind try to continue as normal from new point.
        /// </summary>
        public void CalculateTicksToProcess()
        {
            TicksToProcess = 1;
            long msElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _msStartTime;
            double msPerTick = 1000.0 / Fps;
            double msBehind = msElapsed - (TicksCurrent * msPerTick);
            if (msBehind > MaxMSFallBehindCutoff)
            {
                _msStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                TicksCurrent = 0;
            }
            else if (msBehind > msPerTick)
            {
                TicksToProcess = msBehind > MaxMSCatchUpPerTick ? (int)(MaxMSCatchUpPerTick / msPerTick) : (int)(msBehind / msPerTick);
            }
            TicksCurrent += TicksToProcess;
            _ticksTotal +=  TicksToProcess;
        }

        /// <summary>
        /// Check if two rectangles overlap
        /// </summary>
        public static bool RectangleOverlaps(Rectangle rectangleOne, Rectangle rectangleTwo)
        {
            return rectangleOne.IntersectsWith(rectangleTwo);
        }

        /// <summary>
        /// Convert a number from per second to per frame
        /// </summary>
        public static double ConvertPerSecondToPerFrame(double numberPerSecond)
        {
            return numberPerSecond / Fps;
        }

        /// <summary>
        /// Return a local directory copy of resource file - Create if not exists
        /// </summary>
        public static dynamic GetResourceAsLocal(dynamic file, string fileName)
        {
            string fullPath = AppDomain.CurrentDomain.BaseDirectory + @"/" + fileName;
            if (!File.Exists(fullPath))
            {
                using var fileStream = File.Create(fullPath);
                file.CopyTo(fileStream);
            }
            return fullPath;
        }

        #endregion
    }
}
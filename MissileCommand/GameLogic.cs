using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;
using MissileCommand.Entities;

namespace MissileCommand
{
    public enum GameState
    {
        Over,
        InPlay,
        HighScore,
        Attract
    };

    public class GameLogic : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        float GameScale = 1.91f;

        Background TheBackground;
        EnemyMissileController TheMissiles;
        Player ThePlayer;
        Bomber TheBomber;
        Satellite TheSatellite;
        SmartBomb TheSmartBomb;
        UI TheUI;
        GameState GameMode = GameState.Attract;

        Timer FPSTimer;
        Timer BomberRunTimer;
        Timer SatelliteRunTimer;
        Timer SmartBombRunTimer;
        Timer RadarSoundTimer;

        SoundEffect RadarSound;

        Vector3 TheEnemyColor = new Vector3(1, 0, 0);
        Vector3 ThePlayerColor = new Vector3(0.2f, 0.1f, 2.5f);

        float FPSFrames = 0;

        int Score;

        public GameState CurrentMode { get => GameMode; }
        public Background BackgroundRef { get => TheBackground; }
        public Player PlayerRef { get => ThePlayer; }
        public EnemyMissileController MissilesRef { get => TheMissiles; }
        public Bomber BomberRef { get => TheBomber; }
        public Satellite SatelliteRef { get => TheSatellite; }
        public Timer BomberTimer { get => BomberRunTimer; }
        public Timer SatetlliteTimer { get => SatelliteRunTimer; }
        public Timer SmartBombTimer { get => SmartBombRunTimer; }
        public int GameScore { get => Score; }

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, GameScale);
            ThePlayer = new Player(game, this, GameScale);
            TheMissiles = new EnemyMissileController(game, this, GameScale);
            TheBomber = new Bomber(game, this, GameScale);
            TheSatellite = new Satellite(game, this, GameScale);
            TheSmartBomb = new SmartBomb(game, this, GameScale);
            TheUI = new UI(game, this, GameScale);

            FPSTimer = new Timer(game, 1);
            BomberRunTimer = new Timer(game);
            SatelliteRunTimer = new Timer(game);
            SmartBombRunTimer = new Timer(game);
            RadarSoundTimer = new Timer(game);

            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place objects behind other objects, put them in the negative.
            game.Components.Add(this);
        }

        public override void Initialize()
        {

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {
            RadarSound = PlayerRef.LoadSoundEffect("Radar");
        }

        public void BeginRun()
        {
            RadarSoundTimer.Amount = RadarSound.Duration.Seconds;
        }

        public override void Update(GameTime gameTime)
        {
            FPSFrames++;

            if(FPSTimer.Expired)
            {
                FPSTimer.Reset();
                System.Diagnostics.Debug.WriteLine(FPSFrames.ToString());
                FPSFrames = 0;
            }

            GameStateSwitch();

            base.Update(gameTime);
        }

        public void ScoreUpdate(int score)
        {
            if (GameMode == GameState.InPlay)
            {
                int muliplier = (int)(TheMissiles.Wave / 2.1f) + 1;
                Score += (score * muliplier);
                TheUI.ScoreDisplay.UpdateNumber(Score);
            }
        }

        public void GameOver()
        {
            MissilesRef.GameOver();
            PlayerRef.GameOver();
            BackgroundRef.GameOver();
            TheBomber.Active = false;
            TheSatellite.Active = false;
            GameMode = GameState.Over;
        }

        void GameStateSwitch()
        {
            switch (GameMode)
            {
                case GameState.InPlay:
                    GamePlay();
                    break;
                case GameState.Over:
                    GameOver();
                    break;
                case GameState.Attract:
                    MainMenu();
                    break;
                case GameState.HighScore:
                    HighScore();
                    break;
            }
        }

        void MainMenu()
        {

        }

        void HighScore()
        {

        }

        void GamePlay()
        {
            BomberSatelliteControl();
        }

        void BomberSatelliteControl()
        {
            if (MissilesRef.MissilesLaunched < MissilesRef.MaxMissiles)
            {
                float spawnX;

                if (Services.RandomMinMax(0, 100) > 50)
                {
                    spawnX = -600;
                }
                else
                {
                    spawnX = 600;
                }

                if (BomberRunTimer.Expired && TheMissiles.Wave > 0)
                {
                    BomberRunTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheBomber.Active)
                            BomberRun(spawnX);
                    }
                }

                if (SatelliteRunTimer.Expired && TheMissiles.Wave > 1)
                {
                    SatelliteRunTimer.Reset(Services.RandomMinMax(10.0f, 20.0f));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheSatellite.Active)
                            SatelliteRun(spawnX);
                    }
                }

                if (SmartBombRunTimer.Expired && TheMissiles.Wave > 5)
                {
                    SmartBombRunTimer.Reset(Services.RandomMinMax(10, 20));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheSmartBomb.Active)
                        {
                            TheSmartBomb.Spawn(new Vector3(Services.RandomMinMax(-400, 400), 550, 0));
                            TheSmartBomb.DefuseColor = TheEnemyColor;
                        }
                    }
                }
            }

            if (GameMode == GameState.InPlay)
            {
                if (TheBomber.Active || TheSatellite.Active)
                {
                    if (RadarSoundTimer.Expired)
                    {
                        RadarSoundTimer.Reset();
                        RadarSound.Play();
                    }
                }
            }
        }

        void BomberRun(float spawnX)
        {
            Spawn(new Vector3(spawnX, Services.RandomMinMax(50.0f, 250.0f), 0), TheBomber, TheBomber.DropTimer);
            TheBomber.Spawn();
            TheBomber.DefuseColor = new Vector3(1, 0, 0);
        }

        void SatelliteRun(float spawnX)
        {
            Spawn(new Vector3(spawnX, Services.RandomMinMax(300.0f, 400.0f), 0), TheSatellite, TheSatellite.DropTimer);
            TheSatellite.Spawn();
            TheSatellite.DefuseColor = new Vector3(1, 0, 0);
        }

        public void Spawn(Vector3 position, PositionedObject po, Timer timer)
        {
            timer.Reset(Services.RandomMinMax(10.0f, 35.0f));
            po.Position = position;
            po.Active = true;
            po.Hit = false;

            if (po.Position.X < 0)
            {
                po.Velocity.X = 20;
            }
            else
            {
                po.Velocity.X = -20;
            }

        }

        public void CheckCollusion(PositionedObject po, Timer timer)
        {
            foreach (Explosion explode in PlayerRef.Explosions)
            {
                if (explode.Active)
                {
                    if (po.CirclesIntersect(explode))
                    {
                        ScoreUpdate(100);
                        PlayerRef.SetExplode(po.Position);
                        timer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                        po.Hit = true;
                        break;
                    }
                }
            }
        }

        public void DropBombs(Vector3 position, int numberOfMissiles)
        {
            for (int i = 0; i < numberOfMissiles; i++)
            {
                Vector3 adjust = new Vector3(0, -10, 0) + position;

                TheMissiles.FireMissile(adjust, TheMissiles.ChoseTarget(), TheMissiles.MissileSpeed);
            }
        }

        public void NewGame()
        {
            Score = 0;
            ScoreUpdate(0);
            BackgroundRef.NewGame();
            MissilesRef.NewGame();
            PlayerRef.NewGame();
            GameMode = GameState.InPlay;
        }
    }
}

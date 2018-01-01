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
        Bonus,
        BonusPoints,
        BonusCity,
        BonusCityAwarded,
        HighScore,
        Attract
    };

    public enum DefuseColor
    {
        Red,
        Blue,
        Yellow,
        Green,
        Black
    }

    public enum BackGroundColor
    {
        Black,
        Blue,
        LightBlue,
        Purple,
        Yellow
    }

    public class GameLogic : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        float TheGameScale = 1.91f;

        Background TheBackground;
        EnemyMissileController TheMissiles;
        Player ThePlayer;
        Bomber TheBomber;
        Satellite TheSatellite;
        SmartBomb TheSmartBomb;
        UILogic TheUI;
        List<City> OpenCities = new List<City>();
        List<City> ActiveCities = new List<City>();
        List<AModel> ActiveMissiles = new List<AModel>();

        GameState GameMode = GameState.Attract;
        DefuseColor TheDefuseColor;
        BackGroundColor TheBackColor;

        Timer BomberRunTimer;
        Timer SatelliteRunTimer;
        Timer SmartBombRunTimer;
        Timer RadarSoundTimer;
        Timer BonusCitySoundTimer;

        SoundEffect RadarSound;
        SoundEffect NewCitySound;

        Color TheBackgroundColor;
        Vector3 TheEnemyColor;
        Vector3 ThePlayerColor;
        Vector3 TheGroundColor;
        Vector3[] TheLevelColors = new Vector3[5];
        Color[] BackgroundColors = new Color[5];

        int Score = 0;
        int NextNewCity = 0;
        int NewCityAmount = 8000; //TODO: Normally 10000.
        int NewCityCount = 0;

        bool BonusCityAwarded = false;

        public GameState CurrentMode { get => GameMode; }
        public Background BackgroundRef { get => TheBackground; }
        public Player PlayerRef { get => ThePlayer; }
        public EnemyMissileController MissilesRef { get => TheMissiles; }
        public Bomber BomberRef { get => TheBomber; }
        public Satellite SatelliteRef { get => TheSatellite; }
        public SmartBomb SmartBombRef { get => TheSmartBomb; }
        public List<City> TheActiveCities { get => ActiveCities; }
        public List<AModel> TheActiveMissiles { get => ActiveMissiles; }
        public Vector3[] LevelColors { get => TheLevelColors; }
        public Vector3 EnemyColor { get => TheEnemyColor; }
        public Vector3 GroundColor { get => TheGroundColor; }
        public Vector3 PlayerColor { get => ThePlayerColor; }
        public Color Background { get => TheBackgroundColor; }
        public float GameScale { get => TheGameScale; }
        public int GameScore { get => Score; }
        public int BonusCityAmount { get => NewCityAmount; }

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, this);
            ThePlayer = new Player(game, this);
            TheMissiles = new EnemyMissileController(game, this);
            TheBomber = new Bomber(game, this);
            TheSatellite = new Satellite(game, this);
            TheSmartBomb = new SmartBomb(game, this);
            TheUI = new UILogic(game, this);

            BomberRunTimer = new Timer(game);
            SatelliteRunTimer = new Timer(game);
            SmartBombRunTimer = new Timer(game);
            RadarSoundTimer = new Timer(game);
            BonusCitySoundTimer = new Timer(game);

            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place objects behind other objects, put them in the negative.
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            TheLevelColors[0] = new Vector3(0.01f, 0.0f, 0.01f);
            TheLevelColors[1] = new Vector3(0.286f, 0.286f, 1.5f);
            TheLevelColors[2] = new Vector3(0.3f, 1, 0.129f);
            TheLevelColors[3] = new Vector3(1, 0, 0);
            TheLevelColors[4] = new Vector3(1, 1, 0);

            BackgroundColors[0] = new Color(0.01f, 0.0f, 0.01f);
            BackgroundColors[1] = new Color(0.1f, 0, 1);
            BackgroundColors[2] = new Color(0.329f, 0.56f, 1);
            BackgroundColors[3] = new Color(0.537f, 0.31f, 1);
            BackgroundColors[4] = new Color(0.721f, 0.807f, 0.07f);

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {
            RadarSound = Services.LoadSoundEffect("Radar");
            NewCitySound = Services.LoadSoundEffect("NewCityTune");
        }

        public void BeginRun()
        {
            RadarSoundTimer.Amount = (float)RadarSound.Duration.TotalSeconds;
            BonusCitySoundTimer.Amount = (float)NewCitySound.Duration.TotalSeconds + 1;
            TheGroundColor = TheLevelColors[4];
            ThePlayerColor = TheLevelColors[1];
            TheEnemyColor = TheLevelColors[3];

            foreach(AModel plot in BackgroundRef.Ground)
            {
                plot.DefuseColor = TheGroundColor;
            }

            TheUI.ChangeColors(ThePlayerColor, TheEnemyColor);
            TheUI.HighScores.ChangeColor(ThePlayerColor, TheEnemyColor);
            TheUI.WaveComplete.ChangeColors(ThePlayerColor, TheEnemyColor);
        }

        public override void Update(GameTime gameTime)
        {
            GameStateSwitch();

            base.Update(gameTime);
        }

        public void ScoreUpdate(int score) //TODO: Move multiplier to function to call score count/missiles.
        {
            int muliplier = (int)(TheMissiles.Wave / 2.1f) + 1;
            Score += (score * muliplier);
            TheUI.ScoreDisplay.ChangeNumber(Score, TheEnemyColor);
        }

        public void SwitchToAttract()
        {
            GameMode = GameState.Attract;
        }

        public void GameOver()
        {
            TheUI.WaveComplete.BonusPoints.ShowWords(false);
            MissilesRef.GameOver();
            PlayerRef.GameOver();
            BackgroundRef.GameOver();
            TheBomber.Active = false;
            TheSatellite.Active = false;
            TheUI.WaveComplete.HideDisplay();

            if (TheUI.HighScores.CheckHighScore(Score))
            {
                GameMode = GameState.HighScore;
            }
            else
            {
                GameMode = GameState.Attract;
            }
        }

        public void Bonus()
        {
            GameMode = GameState.Bonus;

            foreach (Explosion explode in PlayerRef.Explosions)
            {
                if (explode.Active)
                    return;
            }

            GameMode = GameState.BonusPoints;

            int missileCount = 0;
            int cityCount = 0;

            ActiveMissiles.Clear();

            foreach (MissileBase silo in BackgroundRef.Bases)
            {
                foreach (AModel acm in silo.Missiles)
                {
                    if (acm.Active)
                    {
                        missileCount++;
                        ActiveMissiles.Add(acm);
                    }
                }
            }

            foreach (City city in BackgroundRef.Cities)
            {
                if (city.Active)
                {
                    ActiveCities.Add(city);
                    cityCount++;
                }
                else
                {
                    OpenCities.Add(city);
                }
            }

            TheUI.WaveComplete.Bonus(missileCount, cityCount);
        }

        void GameStateSwitch()
        {
            switch (GameMode)
            {
                case GameState.InPlay:
                    GamePlay();
                    break;
                case GameState.Bonus:
                    Bonus();
                    break;
                case GameState.BonusPoints:
                    BonusPoints();
                    break;
                case GameState.BonusCity:
                    BonusCity();
                    break;
                case GameState.BonusCityAwarded:
                    BonusCityAward();
                    break;
                case GameState.Attract:
                    MainMenu();
                    break;
                case GameState.HighScore:
                    HighScore();
                    break;
            }
        }

        void NewWave()
        {
            TheUI.WaveComplete.HideDisplay();
            BomberRunTimer.Reset(3);
            SatelliteRunTimer.Reset(5);
            SetTheGameColors();

            foreach(City city in TheBackground.Cities)
            {
                city.DefuseColor = ThePlayerColor;
            }

            foreach (City city in ActiveCities)
            {
                city.Active = true;
                city.Visable = true;
            }

            TheUI.ChangeColors(ThePlayerColor, TheEnemyColor);
            TheUI.HighScores.ChangeColor(ThePlayerColor, TheEnemyColor);
            TheUI.WaveComplete.ChangeColors(ThePlayerColor, TheEnemyColor);

            ActiveCities.Clear();
            BackgroundRef.NewWave();
            MissilesRef.StartWave(TheEnemyColor);
            PlayerRef.DefuseColor = ThePlayerColor;
            GameMode = GameState.InPlay;
        }

        void SetTheGameColors()
        {
            int wave = MissilesRef.Wave;

            if (wave > 14)
                wave -= 14;

            if (wave > 28)
                wave -= 28;

            switch (wave)
            {
                case 0:
                    ThePlayerColor = TheLevelColors[1];
                    TheEnemyColor = TheLevelColors[3];
                    TheGroundColor = TheLevelColors[4];
                    TheBackgroundColor = BackgroundColors[0];
                    break;
                case 2:
                    ThePlayerColor = TheLevelColors[1];
                    TheEnemyColor = TheLevelColors[2];
                    TheGroundColor = TheLevelColors[4];
                    break;
                case 4:
                    ThePlayerColor = TheLevelColors[2];
                    TheEnemyColor = TheLevelColors[3];
                    TheGroundColor = TheLevelColors[1];
                    break;
                case 6:
                    ThePlayerColor = TheLevelColors[1];
                    TheEnemyColor = TheLevelColors[4];
                    TheGroundColor = TheLevelColors[3];
                    break;
                case 8:
                    ThePlayerColor = TheLevelColors[0];
                    TheEnemyColor = TheLevelColors[3];
                    TheGroundColor = TheLevelColors[4];
                    TheBackgroundColor = BackgroundColors[1];
                    break;
                case 10:
                    ThePlayerColor = TheLevelColors[1];
                    TheEnemyColor = TheLevelColors[0];
                    TheGroundColor = TheLevelColors[4];
                    TheBackgroundColor = BackgroundColors[2];
                    break;
                case 12:
                    ThePlayerColor = TheLevelColors[4];
                    TheEnemyColor = TheLevelColors[0];
                    TheGroundColor = TheLevelColors[2];
                    TheBackgroundColor = BackgroundColors[3];
                    break;
                case 14:
                    ThePlayerColor = TheLevelColors[3];
                    TheEnemyColor = TheLevelColors[0];
                    TheGroundColor = TheLevelColors[2];
                    TheBackgroundColor = BackgroundColors[4];
                    break;
            }
        }

        void BonusPoints()
        {
            if (TheUI.WaveComplete.Done)
            {
                GameMode = GameState.BonusCity;
            }
        }

        void BonusCityAward()
        {
            if (!TheUI.WaveComplete.Done)
            {
                return;
            }

            if (!BonusCityAwarded)
            {
                BonusCityAwarded = true;
                NewCitySound.Play();
                TheUI.WaveComplete.BonusCity.ShowWords(true);
                BonusCitySoundTimer.Reset();
            }

            if (BonusCitySoundTimer.Expired)
            {
                TheUI.WaveComplete.BonusCity.ShowWords(false);
                BonusCityAwarded = false;
                NewWave();
            }
        }

        void BonusCity()
        {
            bool newCityAwarded = false;

            if (GameScore > NextNewCity)
            {
                NextNewCity += NewCityAmount;
                NewCityCount++;
                newCityAwarded = true;
            }

            if (OpenCities.Count > 5 && NewCityCount < 1)
            {
                GameOver();
                OpenCities.Clear();
                return;
            }

            for (int i = 0; i < OpenCities.Count; i++)
            {
                if (NewCityCount > 0)
                {
                    int newCity = Services.RandomMinMax(0, OpenCities.Count - 1);
                    ActiveCities.Add(OpenCities[newCity]);
                    OpenCities.RemoveAt(newCity);
                    NewCityCount--;
                }
            }

            OpenCities.Clear();

            if (newCityAwarded)
            {
                GameMode = GameState.BonusCityAwarded;
                return;
            }

            NewWave();
        }

        void MainMenu()
        {
            TheUI.HighScores.ShowHighScoreList(true);
            GameMode = GameState.Over;
        }

        void PutIntoAtrackMode()
        {
            GameMode = GameState.Attract;
        }

        void HighScore()
        {

        }

        void GamePlay()
        {
            BomberSatSmartControl();
        }

        public void ResetBomberTimer()
        {
            BomberRunTimer.Reset(Services.RandomMinMax(5.0f, 10.0f));
        }

        public void ResetSatelliteTimer()
        {
            SatelliteRunTimer.Reset(Services.RandomMinMax(3.0f, 8.0f));
        }

        public void ResetSmartBombTimer()
        {
            SmartBombRunTimer.Reset(Services.RandomMinMax(7.0f, 15.0f));
        }

        void BomberSatSmartControl()
        {
            if (MissilesRef.MissilesLaunched < MissilesRef.MaxMissiles)
            {
                if (BomberRunTimer.Expired && TheMissiles.Wave > 0)
                {
                    if (!TheBomber.Active)
                        BomberRun();
                }

                if (SatelliteRunTimer.Expired && TheMissiles.Wave > 1)
                {
                    if (!TheSatellite.Active)
                        SatelliteRun();
                }

                if (SmartBombRunTimer.Expired && TheMissiles.Wave > 5)
                {
                    if (!TheSmartBomb.Active)
                    {
                        if (IsThereNoMissiles())
                            return;

                        TheSmartBomb.Spawn(new Vector3(Services.RandomMinMax(-400, 400), 550, 0));
                        TheSmartBomb.DefuseColor = TheEnemyColor;
                    }
                }
            }

            if (TheBomber.Active && !TheBomber.Hit || TheSatellite.Active)
            {
                if (RadarSoundTimer.Expired)
                {
                    RadarSoundTimer.Reset();
                    RadarSound.Play();
                }
            }
        }

        float ChoseXSide()
        {
            if (Services.RandomMinMax(0, 100) > 50)
            {
                return -600;
            }
            else
            {
                return 600;
            }
        }

        bool IsThereNoMissiles()
        {
            int missactive = 0;

            foreach (Missile missile in TheMissiles.MissileRef)
            {
                if (missile.Active)
                    missactive++;
            }

            if (missactive < 4)
                return true;

            return false;
        }

        void BomberRun()
        {
            if (IsThereNoMissiles())
                return;

            Spawn(new Vector3(ChoseXSide(), Services.RandomMinMax(50.0f, 250.0f), 0),
                TheBomber, TheBomber.DropTimer);
            TheBomber.Spawn(TheEnemyColor);
        }

        void SatelliteRun()
        {
            if (IsThereNoMissiles())
                return;

            Spawn(new Vector3(ChoseXSide(), Services.RandomMinMax(300.0f, 400.0f), 0),
                TheSatellite, TheSatellite.DropTimer);
            TheSatellite.Spawn(TheEnemyColor);
        }

        public void NoCities()
        {
            TheSatellite.Velocity *= 10;
            TheBomber.Velocity *= 10;
            TheSmartBomb.Velocity *= 10;
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

        public void CheckCollusion(PositionedObject po)
        {
            foreach (Explosion explode in PlayerRef.Explosions)
            {
                if (explode.Active)
                {
                    if (po.CirclesIntersect(explode))
                    {
                        ScoreUpdate(100);
                        PlayerRef.SetExplode(po.Position);
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
            NewCityCount = 0;
            NextNewCity = NewCityAmount;
            ScoreUpdate(0);
            BackgroundRef.NewGame();
            MissilesRef.NewGame();
            PlayerRef.NewGame();
            TheUI.NewGame();
            GameMode = GameState.InPlay;
            NewWave();
        }
    }
}

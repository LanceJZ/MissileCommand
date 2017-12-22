using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using Engine;

namespace MissileCommand.Entities
{
    enum WaveMode
    {
        CountMissiles,
        CountCities,
        End
    }

    public class WaveCompleteUI : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        GameLogic GameLogicRef;

        Words BonusPointsText;
        Words BonusCityText;
        Numbers MissilePoints;
        Numbers CityPoints;
        List<AModel> MissileModels;
        List<AModel> CityModels;
        XnaModel CityModel;
        XnaModel MissileModel;
        Timer MissileCountTimer;
        Timer CityCountTimer;
        Timer EndTimer;

        SoundEffect CountSound;

        WaveMode Currentmode;

        int MissileCount;
        int BonusMissileAmount;
        int CityCount;
        int BonusCityAmount;
        bool IsDone = false;

        public Words BonusCity { get => BonusCityText; }
        public bool Done { get => IsDone; }

        public WaveCompleteUI(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;
            BonusPointsText = new Words(game);
            BonusCityText = new Words(game);
            MissilePoints = new Numbers(game);
            CityPoints = new Numbers(game);
            MissileModels = new List<AModel>();
            CityModels = new List<AModel>();

            MissileCountTimer = new Timer(game);
            CityCountTimer = new Timer(game);
            EndTimer = new Timer(game, 3);

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
            MissileModel = Services.LoadModel("MC_MissileAmmo");
            CityModel = Services.LoadModel("MC_CityGS");
            CountSound = Services.LoadSoundEffect("Score Count");
        }

        public void BeginRun()
        {
            for (int i = 0; i < 6; i++)
            {
                CityModels.Add(new AModel(Game, CityModel));
                CityModels.Last().Active = false;
                CityModels.Last().Position.Y = -50;
                CityModels.Last().Position.X = i * (CityModels.Last().SphereRadius * 3);
                CityModels.Last().DefuseColor = GameLogicRef.PlayerRef.DefuseColor;
                CityModels.Last().MatrixUpdate();
            }

            for (int i = 0; i < 30; i++)
            {
                MissileModels.Add(new AModel(Game, MissileModel));
                MissileModels.Last().Active = false;
                MissileModels.Last().Position.Y = 50;
                MissileModels.Last().Position.X = i * (MissileModels.Last().SphereRadius * 3);
                MissileModels.Last().DefuseColor = GameLogicRef.PlayerRef.DefuseColor;
                MissileModels.Last().MatrixUpdate();
            }

            BonusPointsText.ProcessWords("BONUS POINTS", new Vector3(-120, 200, 10), 2);
            BonusCityText.ProcessWords("BONUS CITY", new Vector3(-100, -200, 10), 2);
            BonusPointsText.ShowWords(false);
            BonusCityText.ShowWords(false);
            MissilePoints.ProcessNumber(0, new Vector3(-40, 50, 10), 2);
            CityPoints.ProcessNumber(0, new Vector3(-60, -50, 10), 2);
            MissilePoints.ShowNumbers(false);
            CityPoints.ShowNumbers(false);
            MissileCountTimer.Amount = (float)CountSound.Duration.TotalSeconds;
            CityCountTimer.Amount = MissileCountTimer.Amount + 0.25f;
        }

        public override void Update(GameTime gameTime)
        {
            switch (GameLogicRef.CurrentMode)
            {
                case GameState.BonusPoints:
                    switch (Currentmode)
                    {
                        case WaveMode.CountMissiles:
                            if (MissileCount < 1)
                            {
                                Currentmode = WaveMode.CountCities;
                                return;
                            }

                            if (MissileCountTimer.Expired)
                            {
                                MissileCountTimer.Reset();
                                CountSound.Play();
                                MissilePoints.ShowNumbers(true);
                                CountTheMissiles();
                            }
                            break;
                        case WaveMode.CountCities:
                            if (CityCount < 1)
                            {
                                Currentmode = WaveMode.End;
                                return;
                            }

                            if (CityCountTimer.Expired)
                            {
                                CityCountTimer.Reset();
                                CountSound.Play();
                                CityPoints.ShowNumbers(true);
                                CountTheCities();
                            }

                            EndTimer.Reset();
                            break;
                        case WaveMode.End:
                            if (EndTimer.Expired)
                            {
                                BonusPointsDisplayEnd();
                                IsDone = true;
                            }
                            break;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        public void Bonus(int missileCount, int cityCount)
        {
            MissileCount = missileCount;
            BonusMissileAmount = 0;
            CityCount = cityCount;
            BonusCityAmount = 0;
            IsDone = false;
            BonusPointsText.ShowWords(true);
            Currentmode = WaveMode.CountMissiles;
        }

        void BonusPointsDisplayEnd()
        {
            foreach(AModel city in CityModels)
            {
                city.Active = false;
            }

            foreach(AModel missile in MissileModels)
            {
                missile.Active = false;
            }

            GameLogicRef.ScoreUpdate(BonusMissileAmount + BonusCityAmount);
            BonusPointsText.ShowWords(false);
            MissilePoints.ShowNumbers(false);
            CityPoints.ShowNumbers(false);
        }

        void CountTheMissiles()
        {
            int countedSoFar = 0;

            foreach (AModel missile in MissileModels)
            {
                if (missile.Active)
                {
                    countedSoFar++;
                }
            }

            if (countedSoFar < MissileCount)
            {
                MissileModels[countedSoFar].Active = true;
                BonusMissileAmount += 5;
                MissilePoints.UpdateNumber(BonusMissileAmount);
            }
            else
            {
                Currentmode = WaveMode.CountCities;
            }
        }

        void CountTheCities()
        {
            int countedSoFar = 0;

            foreach(AModel city in CityModels)
            {
                if (city.Active)
                {
                    countedSoFar++;
                }
            }

            if (countedSoFar < CityCount)
            {
                CityModels[countedSoFar].Active = true;
                BonusCityAmount += 100;
                CityPoints.UpdateNumber(BonusCityAmount);
            }
            else
            {
                Currentmode = WaveMode.End;
            }
        }
    }
}

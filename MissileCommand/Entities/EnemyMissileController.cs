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
    public struct MissileTarget
    {
        public float Min;
        public float Max;
    }

    public class EnemyMissileController : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        List<Missile> TheMissiles;
        Background BackgroundRef;
        Player PlayerRef;
        GameLogic GameLogicRef;

        Timer FireTimer;

        SoundEffect NewWaveSound;

        MissileTarget[] TargetCities = new MissileTarget[6];
        MissileTarget[] TargetBases = new MissileTarget[3];
        MissileTarget[] TargetLand = new MissileTarget[5];

        int[] TargetedCities = new int[3];

        Vector3 TheColor = new Vector3(1, 0, 0);

        float GameScale;
        int MaxNumberOfMissiles = 0;
        int LaunchedMissiles = 0;
        int TheMissileSpeed = 20;
        int TheWave = 0;
        int NextNewCity = 0;
        int NewCityAmount = 10000;
        int NewCityCount = 0;

        bool Active = true;

        public List<Missile> MissileRef { get => TheMissiles; }
        public int MissileSpeed { get => TheMissileSpeed; }
        public int MissilesLaunched { get => LaunchedMissiles; }
        public int MaxMissiles { get => MaxNumberOfMissiles; }
        public int Wave { get => TheWave; }

        public EnemyMissileController(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameScale = gameScale;
            TheMissiles = new List<Missile>();
            GameLogicRef = gameLogic;
            BackgroundRef = gameLogic.BackgroundRef;
            PlayerRef = gameLogic.PlayerRef;
            FireTimer = new Timer(game);

            game.Components.Add(this);
            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place things behind, they are in the negative.
        }

        public override void Initialize()
        {
            foreach (City city in BackgroundRef.Cities)
            {
                int i = 0;
                TargetCities[i].Min = city.Position.X - 20;
                TargetCities[i].Max = city.Position.X + 20;
                i++;
            }

            for (int i = 0; i < 3; i++)
            {
                float startX = BackgroundRef.Bases[i].Position.X;
                TargetBases[i].Min = startX - 10;
                TargetBases[i].Max = startX + 10;
            }

            TargetLand[0].Min = BackgroundRef.Cities[0].Position.X + 60;
            TargetLand[0].Max = BackgroundRef.Cities[1].Position.X - 60;
            TargetLand[1].Min = BackgroundRef.Cities[1].Position.X + 60;
            TargetLand[1].Max = BackgroundRef.Cities[2].Position.X - 60;
            TargetLand[2].Min = BackgroundRef.Cities[3].Position.X + 60;
            TargetLand[2].Max = BackgroundRef.Cities[4].Position.X - 60;
            TargetLand[3].Min = BackgroundRef.Cities[4].Position.X + 60;
            TargetLand[3].Max = BackgroundRef.Cities[5].Position.X - 60;
            TargetLand[4].Min = BackgroundRef.Cities[5].Position.X + 60;
            TargetLand[4].Max = 550 - 60;

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {
            NewWaveSound = PlayerRef.LoadSoundEffect("Start Game");
        }

        public void BeginRun()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (GameLogicRef.CurrentMode == GameState.InPlay)
            {
                if (FireTimer.Expired)
                {
                    float maxTime = 15.0f - (TheWave * 0.25f);
                    float minTime = 5.0f - (TheWave * 0.1f);

                    if (maxTime < 4)
                        maxTime = 4;

                    if (minTime < 1)
                        minTime = 1;

                    FireTimer.Reset(Services.RandomMinMax(minTime, maxTime));
                    LaunchMissile();
                }

                if (Active)
                {
                    Collusions();
                }
            }

            base.Update(gameTime);
        }

        public void FireMissile(Vector3 position, Vector3 target, float speed)
        {
            bool spawnNew = true;
            int freeOne = TheMissiles.Count;

            for (int i = 0; i < TheMissiles.Count; i++)
            {
                if (!TheMissiles[i].Active)
                {
                    spawnNew = false;
                    freeOne = i;
                    break;
                }
            }

            if (spawnNew)
            {
                TheMissiles.Add(new Missile(Game, GameLogicRef, GameScale));
            }

            TheMissiles[freeOne].Spawn(position, target, speed);
            TheMissiles[freeOne].DefuseColor = new Vector3(0.1f, 1, 1);
            TheMissiles[freeOne].TrailColor = new Vector3(1, 0, 0);
        }

        public Vector3 ChoseTarget()
        {
            if (Services.RandomMinMax(0.0f, 100.0f) < 40 + (TheWave * 1.5f))
            {
                return ChoseCitySiloTarget();
            }
            else
            {
                return ChoseLandTarget();
            }
        }

        public Vector3 ChoseLandTarget()
        {
            int land = Services.RandomMinMax(0, 4);
            return new Vector3(Services.RandomMinMax(TargetLand[land].Min,
                TargetLand[land].Max), -400, 0);
        }

        public Vector3 ChoseCitySiloTarget()
        {
            if (Services.RandomMinMax(0.0f, 100.0f) > 50)
            {
                return new Vector3(BackgroundRef.Cities[TargetedCities[Services.RandomMinMax(0, 2)]].Position.X,
                    -400, 0);
            }
            else
            {
                int silo = Services.RandomMinMax(0, 2);
                return new Vector3(Services.RandomMinMax(TargetBases[silo].Min,
                    TargetBases[silo].Max), -400, 0);
            }
        }

        public void NewGame()
        {
            TheWave = 0;
            NewCityCount = 0;
            NextNewCity = NewCityAmount;
            TheMissileSpeed = 20;
            MaxNumberOfMissiles = 10;
            TargetedCities = ChoseCities();
        }

        public void GameOver()
        {
            foreach (Missile missile in TheMissiles)
            {
                missile.Active = false;
            }
        }

        int[] ChoseCities()
        {
            NewWaveSound.Play();

            int[] targetedCities = new int[3];

            for (int i = 0; i < 3; i++)
            {
                targetedCities[i] = 6;
            }

            for (int city = 0; city < 3; city++)
            {
                bool nextCity = false;
                int cityTarget = 0;

                while (!nextCity)
                {
                    nextCity = true;
                    cityTarget = Services.RandomMinMax(0, 5);

                    for (int i = 0; i < 3; i++)
                    {
                        if (i != city)
                        {
                            if (cityTarget == targetedCities[i])
                            {
                                nextCity = false;
                                break;
                            }
                        }
                    }
                }

                targetedCities[city] = cityTarget;
            }

            return targetedCities;
        }

        void LaunchMissile()
        {
            if (LaunchedMissiles < MaxNumberOfMissiles + 4 ||
                GameLogicRef.BomberRef.Active || GameLogicRef.SatelliteRef.Active)
            {
                if (LaunchedMissiles < MaxNumberOfMissiles + 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        FireMissile(new Vector3(Services.RandomMinMax(-300, 300), 450, 0), ChoseTarget(), TheMissileSpeed);
                        LaunchedMissiles++;
                    }
                }
                else
                {

                    GameLogicRef.BomberTimer.Reset(20);
                    GameLogicRef.SatetlliteTimer.Reset(20);
                }
            }
            else
            {
                if (CheckForActiveMissiles())
                {
                    LaunchedMissiles = 0;
                    GameLogicRef.BomberTimer.Reset(15);
                    GameLogicRef.SatetlliteTimer.Reset(12);

                    //System.Diagnostics.Debug.WriteLine("Wave: " + TheWave.ToString());

                    foreach (MissileBase silo in GameLogicRef.BackgroundRef.Bases)
                    {
                        foreach (AModel acm in silo.Missiles)
                        {
                            if (acm.Active)
                                GameLogicRef.ScoreUpdate(25);
                        }
                    }

                    GameLogicRef.BackgroundRef.NewWave(new Vector3(0.2f, 0.1f, 2.5f));

                    List<City> openCities = new List<City>();

                    foreach (City city in GameLogicRef.BackgroundRef.Cities)
                    {
                        if (city.Active)
                        {
                            GameLogicRef.ScoreUpdate(100);
                        }
                        else
                        {
                            openCities.Add(city);
                        }
                    }

                    if (GameLogicRef.GameScore > NextNewCity)
                    {
                        NextNewCity += NewCityAmount;
                        NewCityCount++;
                    }

                    if (openCities.Count > 5 && NewCityCount < 1)
                    {
                        GameLogicRef.GameOver();
                        return;
                    }

                    for (int i = 0; i < openCities.Count; i++)
                    {
                        if (NewCityCount > 0)
                        {
                            openCities[Services.RandomMinMax(0, openCities.Count - 1)].Active = true;
                            NewCityCount--;
                            openCities.Clear();

                            foreach (City city in GameLogicRef.BackgroundRef.Cities)
                            {
                                if (!city.Active)
                                {
                                    openCities.Add(city);
                                }
                            }
                        }
                    }

                    TargetedCities = ChoseCities();
                    TheMissileSpeed += 3;
                    MaxNumberOfMissiles += 2;
                    TheWave++;
                    FireTimer.Reset(NewWaveSound.Duration.Seconds);
                }
            }
        }

        void Collusions()
        {
            foreach (Missile missile in TheMissiles)
            {
                if (missile.Active)
                {
                    if (missile.Position.Y < -400)
                    {
                        missile.Deactivate();
                        break;
                    }

                    foreach (Explosion explode in PlayerRef.Explosions)
                    {
                        if (explode.Active)
                        {
                            if (missile.CirclesIntersect(explode))
                            {
                                GameLogicRef.ScoreUpdate(25);
                                PlayerRef.SetExplode(missile.Position);
                                missile.Deactivate();
                                return;
                            }
                        }
                    }

                    foreach (City city in BackgroundRef.Cities)
                    {
                        if (city.Active)
                        {
                            if (missile.SphereIntersect2D(city))
                            {
                                missile.Deactivate();
                                city.Deactivate();
                                break;
                            }
                        }
                    }

                    foreach (MissileBase silo in BackgroundRef.Bases)
                    {
                        if (silo.Active)
                        {
                            if (missile.CirclesIntersect(silo))
                            {
                                missile.Deactivate();
                                silo.HitByMissile();
                                break;
                            }
                        }
                    }
                }
            }
        }

        bool CheckForActiveMissiles()
        {
            foreach(Missile missile in TheMissiles)
            {
                if (missile.Active)
                    return false;
            }

            return true;
        }
    }
}

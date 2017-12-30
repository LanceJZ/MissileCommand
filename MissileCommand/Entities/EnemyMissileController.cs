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

        MissileTarget[] TargetBases = new MissileTarget[3];
        MissileTarget[] TargetLand = new MissileTarget[5];

        List<int> TargetedCities = new List<int>();

        Vector3 TheColor;

        int MaxNumberOfMissiles = 0;
        int LaunchedMissiles = 0;
        int TheMissileSpeed = 20;
        int TheWave = 0;

        bool Active = true;

        public List<Missile> MissileRef { get => TheMissiles; }
        public int MissileSpeed { get => TheMissileSpeed; }
        public int MissilesLaunched { get => LaunchedMissiles; }
        public int MaxMissiles { get => MaxNumberOfMissiles; }
        public int Wave { get => TheWave; }

        public EnemyMissileController(Game game, GameLogic gameLogic) : base(game)
        {
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
            TheColor = GameLogicRef.EnemyColor;

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {
            NewWaveSound = Services.LoadSoundEffect("Start Game");
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
                    float maxTime = 6.0f - (TheWave * 0.25f);
                    float minTime = 3.0f - (TheWave * 0.1f);

                    if (maxTime < 2)
                        maxTime = 2;

                    if (minTime < 1)
                        minTime = 1;

                    FireTimer.Reset(Services.RandomMinMax(minTime, maxTime));
                    LaunchMissile();
                }

                Collusions();

                if (LaunchedMissiles >= MaxNumberOfMissiles &&
                    !GameLogicRef.BomberRef.Active && !GameLogicRef.SatelliteRef.Active)
                {
                    if (CheckForActiveMissiles() && !GameLogicRef.SmartBombRef.Active &&
                        !GameLogicRef.BomberRef.Active && !GameLogicRef.SatelliteRef.Active)
                    {
                        PlayerRef.Active = false;
                        GameLogicRef.Bonus();
                    }
                }

                if (Active)
                {
                    foreach (Explosion explode in GameLogicRef.PlayerRef.Explosions)
                    {
                        if (explode.Active)
                            return;
                    }

                    bool noCities = true;

                    foreach (City city in BackgroundRef.Cities)
                    {
                        if (city.Active)
                        {
                            noCities = false;
                            Active = false;
                            break;
                        }
                    }

                    if (noCities)
                    {
                        foreach (Missile missile in MissileRef)
                        {
                            if (!missile.TempSpeedup)
                            {
                                missile.Velocity *= 10;
                                missile.TempSpeedup = true;
                            }
                        }

                        GameLogicRef.NoCities();
                    }

                    bool noMissiles = true;

                    foreach (MissileBase silo in GameLogicRef.BackgroundRef.Bases)
                    {
                        foreach (AModel missile in silo.Missiles)
                        {
                            if (missile.Active)
                            {
                                noMissiles = false;
                                Active = false;
                                break;
                            }
                        }
                    }

                    if (noMissiles)
                    {
                        foreach (Missile missile in MissileRef)
                        {
                            if (!missile.TempSpeedup)
                            {
                                missile.Velocity *= 4;
                                missile.TempSpeedup = true;
                            }
                        }
                    }
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
                TheMissiles.Add(new Missile(Game, GameLogicRef));
            }

            TheMissiles[freeOne].Spawn(position, target, speed, TheColor);
            TheMissiles[freeOne].DefuseColor = new Vector3(0.1f, 1, 1);
        }

        public Vector3 ChoseTarget()
        {
            if (Services.RandomMinMax(0.0f, 100.0f) < 50 + (TheWave * 1.5f) + (TargetedCities.Count * 5))
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
            if (Services.RandomMinMax(0.0f, 100.0f) < 60 + (TargetedCities.Count * 5))
            {
                int target = Services.RandomMinMax(0, TargetedCities.Count - 1);
                return new Vector3(BackgroundRef.Cities[TargetedCities[target]].Position.X, -400, 0);
            }
            else
            {
                int silo = Services.RandomMinMax(0, 2);
                return new Vector3(Services.RandomMinMax(TargetBases[silo].Min, TargetBases[silo].Max), -400, 0);
            }
        }

        public void NewGame()
        {
            TheWave = 0;
            TheMissileSpeed = 20;
            MaxNumberOfMissiles = 12;
        }

        public void GameOver()
        {
            foreach (Missile missile in TheMissiles)
            {
                missile.Active = false;
            }
        }

        public void StartWave(Vector3 defuseColor)
        {
            TheColor = defuseColor;
            PlayerRef.Active = true;
            TargetedCities.Clear();
            TargetedCities = ChoseCities();
            TheMissileSpeed += 3;
            MaxNumberOfMissiles += 2;
            LaunchedMissiles = 0;
            FireTimer.Reset(NewWaveSound.Duration.Seconds);
        }

        public void NextWave()
        {
            TheWave++;
        }

        void LaunchMissile()
        {
            if (LaunchedMissiles < MaxNumberOfMissiles + 4 ||
                GameLogicRef.BomberRef.Active || GameLogicRef.SatelliteRef.Active)
            {
                if (LaunchedMissiles < MaxNumberOfMissiles + 4)
                {
                    Active = true;

                    for (int i = 0; i < 4; i++)
                    {
                        FireMissile(new Vector3(Services.RandomMinMax(-300, 300), 450, 0),
                            ChoseTarget(), TheMissileSpeed);
                        LaunchedMissiles++;
                    }
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

        List<int> ChoseCities()
        {
            NewWaveSound.Play();

            List<int> citiesAvailable = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                if (BackgroundRef.Cities[i].Active)
                {
                    citiesAvailable.Add(i);
                }
            }

            List<int> targetedCities = new List<int>();

            int maxCity = citiesAvailable.Count;

            for (int city = 0; city < maxCity && city < 3; city++)
            {
                int cityTarget = Services.RandomMinMax(0, citiesAvailable.Count - 1);
                targetedCities.Add(citiesAvailable[cityTarget]);
                citiesAvailable.RemoveAt(cityTarget);
            }

            return targetedCities;
        }
    }
}

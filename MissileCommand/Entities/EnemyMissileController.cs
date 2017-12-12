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

        MissileTarget[] TargetCities = new MissileTarget[6];
        MissileTarget[] TargetBases = new MissileTarget[3];
        MissileTarget[] TargetLand = new MissileTarget[5];

        int[] TargetedCities = new int[3];

        float GameScale;
        int MaxNumberOfMissiles = 10;
        int LaunchedMissiles = 0;
        int TheMissileSpeed = 15;
        int TheWave = 0;

        bool Active = true;

        public List<Missile> MissileRef { get => TheMissiles; }
        public int MissileSpeed { get => TheMissileSpeed; }
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

        }

        public void BeginRun()
        {
            TargetedCities = ChoseCities();

        }

        public override void Update(GameTime gameTime)
        {
            if (FireTimer.Expired)
            {
                FireTimer.Reset(Services.RandomMinMax(1, 10));
                LounchMissile();
            }

            if (Active)
            {
                Collusions();
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
                TheMissiles.Add(new Missile(Game, GameScale));
            }

            TheMissiles[freeOne].Spawn(position, target, speed);
            TheMissiles[freeOne].DefuseColor = new Vector3(0.1f, 1, 1);
            TheMissiles[freeOne].TrailColor = new Vector3(1, 0, 0);
        }

        public Vector3 ChoseTarget()
        {
            if (Services.RandomMinMax(0.0f, 100.0f) > 50)
            {
                return new Vector3(BackgroundRef.Cities[TargetedCities[Services.RandomMinMax(0, 2)]].Position.X,
                    -400, 0);
            }

            if (Services.RandomMinMax(0.0f, 100.0f) > 50)
            {
                int silo = Services.RandomMinMax(0, 2);
                return new Vector3(Services.RandomMinMax(TargetBases[silo].Min,
                    TargetBases[silo].Max), -400, 0);
            }

            int land = Services.RandomMinMax(0, 4);
            return new Vector3(Services.RandomMinMax(TargetLand[land].Min,
                TargetLand[land].Max), -400, 0);
        }

        int[] ChoseCities()
        {
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

        void LounchMissile()
        {
            if (LaunchedMissiles < MaxNumberOfMissiles)
            {
                FireMissile(new Vector3(Services.RandomMinMax(-300, 300), 450, 0), ChoseTarget(), TheMissileSpeed);
                LaunchedMissiles++;
            }
            else
            {
                if (CheckForActiveMissiles())
                {
                    LaunchedMissiles = 0;
                    TheMissileSpeed += 5;
                    MaxNumberOfMissiles += 5;
                    TheWave++;

                    foreach (MissileBase silo in GameLogicRef.BackgroundRef.Bases)
                    {
                        silo.Spawn(new Vector3(0.2f, 0.1f, 2.5f));
                    }

                    TargetedCities = ChoseCities();
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
                                GameLogicRef.ScoreUpdate(1);
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

                    foreach (MissileBase missileBase in BackgroundRef.Bases)
                    {
                        if (missileBase.Active)
                        {
                            if (missile.CirclesIntersect(missileBase))
                            {
                                missile.Deactivate();
                                missileBase.Deativate();
                                break;
                            }
                        }
                    }
                }
            }

            if (GameLogicRef.BomberRef.Active)
            {
                foreach (Explosion explode in PlayerRef.Explosions)
                {
                    if (explode.Active)
                    {
                        GameLogicRef.BomberRef.CheckCollusion(explode);
                        break;
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

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
        List<Explosion> TheExplosions;
        Background BackgroundRef;
        Player PlayerRef;

        Timer FireTimer;

        MissileTarget[] TargetCities = new MissileTarget[6];
        MissileTarget[] TargetBases = new MissileTarget[3];
        MissileTarget[] TargetLand = new MissileTarget[5];

        float GameScale;
        int MaxNumberOfMissiles = 50;
        int LaunchedMissiles = 0;
        int Group = 4;

        public EnemyMissileController(Game game, float gameScale, Background background, Player player) : base(game)
        {
            GameScale = gameScale;
            TheMissiles = new List<Missile>();
            TheExplosions = new List<Explosion>();
            BackgroundRef = background;
            PlayerRef = player;
            FireTimer = new Timer(game, 1.2666f);

            // Screen resolution is 1200 X 900. Top Right Corner positive.
            // Y positive on top of window.
            // X Positive is on the right of the window.
            game.Components.Add(this);
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
                float startX = (i * 550) - 550;
                TargetBases[i].Min = startX - 10;
                TargetBases[i].Max = startX + 10;
            }

            TargetLand[0].Min = BackgroundRef.Cities[0].Position.X - 30;
            TargetLand[0].Max = BackgroundRef.Cities[1].Position.X + 30;
            TargetLand[1].Min = BackgroundRef.Cities[1].Position.X - 30;
            TargetLand[1].Max = BackgroundRef.Cities[2].Position.X + 30;
            TargetLand[2].Min = BackgroundRef.Cities[3].Position.X + 30;
            TargetLand[2].Max = BackgroundRef.Cities[4].Position.X - 30;
            TargetLand[3].Min = BackgroundRef.Cities[4].Position.X + 30;
            TargetLand[3].Max = BackgroundRef.Cities[5].Position.X - 30;
            TargetLand[4].Min = BackgroundRef.Cities[5].Position.X + 30;
            TargetLand[4].Max = 550 - 30;


            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {

        }

        public void BeginRun()
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (FireTimer.Expired)
            {
                FireTimer.Reset();

                if (LaunchedMissiles < MaxNumberOfMissiles)
                {
                    FireMissile(Services.RandomMinMax(-400, 400));
                    LaunchedMissiles++;
                }
            }

            if (Collusions())
            {
                LaunchedMissiles = 0;
            }

            base.Update(gameTime);
        }

        void FireMissile(float spawnX)
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

            TheMissiles[freeOne].Spawn(new Vector3(spawnX, 450, 0));
            TheMissiles[freeOne].DefuseColor = new Vector3(0.1f, 1, 1);
            TheMissiles[freeOne].TrailColor = new Vector3(1, 0, 0);
        }

        void SetExplode(Vector3 position)
        {
            bool spawnNew = true;
            int freeOne = TheExplosions.Count;

            for (int i = 0; i < TheExplosions.Count; i++)
            {
                if (!TheExplosions[i].Active)
                {
                    spawnNew = false;
                    freeOne = i;
                    break;
                }
            }

            if (spawnNew)
            {
                TheExplosions.Add(new Explosion(Game, GameScale));
            }

            TheExplosions[freeOne].Spawn(position);
        }

        bool Collusions()
        {
            bool noMissile = true;

            foreach (Missile missile in TheMissiles)
            {
                if (missile.Active)
                {
                    noMissile = false;

                    if (missile.Position.Y < -415)
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
                                SetExplode(missile.Position);
                                missile.Deactivate();
                                break;
                            }
                        }
                    }

                    foreach (Explosion explode in TheExplosions)
                    {
                        if (explode.Active)
                        {
                            if (missile.CirclesIntersect(explode))
                            {
                                SetExplode(missile.Position);
                                missile.Deactivate();
                                break;
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

                    foreach(MissileBase missileBase in BackgroundRef.Bases)
                    {
                        if (missileBase.Active)
                        {
                            if (missile.CirclesIntersect(missileBase))
                            {
                                missile.Deactivate();
                                missileBase.Deativate();
                            }
                        }
                    }
                }
            }

            if (noMissile)
                return true;

            return false;
        }
    }
}

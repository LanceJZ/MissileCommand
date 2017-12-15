using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;

namespace MissileCommand.Entities
{
    public class SmartBomb : AModel
    {
        GameLogic GameLogicRef;
        PositionedObject Radar;

        bool ExplosionDetected;
        Vector3 OriginalTarget;
        Vector3 AvoidTarget;

        public SmartBomb(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameLogicRef = gameLogic;
            GameScale = gameScale;
            Radar = new PositionedObject(game);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Active = false;
            Hit = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_SmartBomb");
        }

        public override void BeginRun()
        {
            Radar.AddAsChildOf(this, true, true);
            Radar.Radius = SphereRadius * 1.5f;
            Radius = SphereRadius;

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                DetectExplosions();
                SetHeading();
            }

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Active = true;
            Hit = false;
            Position = position;
            RotationVelocity.Y = Services.RandomMinMax(-8, 8);
            RotationVelocity.X = Services.RandomMinMax(-8, 8);
            OriginalTarget = GameLogicRef.MissilesRef.ChoseCitySiloTarget();
            ExplosionDetected = false;

            MatrixUpdate();
        }

        public void Deactivate()
        {
            Active = false;
        }

        void DetectExplosions()
        {
            ExplosionDetected = false;

            foreach (Explosion explode in GameLogicRef.PlayerRef.Explosions)
            {
                if (explode.Active)
                {
                    if (Radar.CirclesIntersect(explode))
                    {
                        ExplosionDetected = true;
                        AvoidTarget = explode.Position;
                    }

                    if (CirclesIntersect(explode))
                    {
                        GameLogicRef.PlayerRef.SetExplode(Position);
                        GameLogicRef.ScoreUpdate(125);
                        Active = false;
                        break;
                    }
                }
            }

            foreach (City city in GameLogicRef.BackgroundRef.Cities)
            {
                if (SphereIntersect2D(city))
                {
                    Active = false;

                    if (city.Active)
                    {
                        city.Deactivate();
                        break;
                    }
                }
            }

            foreach (MissileBase silo in GameLogicRef.BackgroundRef.Bases)
            {
                if (CirclesIntersect(silo))
                {
                    Active = false;

                    if (silo.Active)
                    {
                        silo.Deativate();
                        break;
                    }
                }
            }
        }

        void SetHeading()
        {
            if (ExplosionDetected)
            {
                Velocity = VelocityFromVectors(AvoidTarget, Position, 70);
            }
            else
            {
                Velocity = VelocityFromVectors(OriginalTarget, 40);
            }
        }
    }
}

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
    enum RunMode
    {
        Drop,
        Evade
    }

    public class SmartBomb : AModel
    {
        GameLogic GameLogicRef;
        PositionedObject Radar;
        RunMode CurrentMode;
        Vector3 OriginalTarget;
        Vector3 AvoidTarget;

        public SmartBomb(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;
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
            CurrentMode = RunMode.Drop;

            MatrixUpdate();
        }

        public void Deactivate()
        {
            Active = false;
            GameLogicRef.ResetSmartBombTimer();
        }

        void DetectExplosions()
        {
            foreach (Explosion explode in GameLogicRef.PlayerRef.Explosions)
            {
                if (explode.Active)
                {
                    if (Radar.CirclesIntersect(explode))
                    {
                        CurrentMode = RunMode.Evade;
                        AvoidTarget = explode.Position;
                    }
                    else
                    {
                        CurrentMode = RunMode.Drop;
                    }

                    if (CirclesIntersect(explode))
                    {
                        GameLogicRef.PlayerRef.SetExplode(Position);
                        GameLogicRef.ScoreUpdate(125);
                        GameLogicRef.ResetBomberTimer();
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
            switch(CurrentMode)
            {
                case RunMode.Evade:
                Velocity = VelocityFromVectors(AvoidTarget, Position, 70);
                    break;
                case RunMode.Drop:
                    Velocity = VelocityFromVectors(OriginalTarget, 40);
                    break;
            }
        }
    }
}

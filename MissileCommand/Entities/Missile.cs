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
    using Mod = AModel;

    public class Missile : Mod
    {
        Mod Trail;
        Vector3 Target;
        Mod TargetMod;
        Timer TrailTimer;

        public float TimerAmount
        {
            set => TrailTimer.Amount = value;
        }

        public Vector3 TrailColor
        {
            set => Trail.DefuseColor = value;
        }

        public Missile(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            Trail = new Mod(game);
            TrailTimer = new Timer(game, 0.2666f);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            ModelScale = new Vector3(1);
            Active = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_WarHead");
            Trail.LoadModel("MC_WarHeadTrail");
        }

        public override void BeginRun()
        {
            Trail.Moveable = false;
            Trail.ModelScale = new Vector3(1.5f);

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                if (TrailTimer.Expired && TrailTimer.Enabled)
                {
                    TrailTimer.Reset();
                    Trail.ModelScale = new Vector3(Vector3.Distance(Trail.Position, Position), 1.5f, 1);
                }

                if (Colusions())
                    Deactivate();
            }

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Spawn(position, new Vector3(Services.RandomMinMax(-480, 450), -450, 0), 10);
        }

        public void Spawn(Vector3 position, Mod target, float speed)
        {
            TargetMod = target;

            Spawn(position, target.Position, speed);
        }

        public void Spawn(Vector3 position, Vector3 target, float speed)
        {
            Position = position;
            Target = target;
            Active = true;
            TrailTimer.Enabled = true;
            Trail.Active = true;
            Trail.ModelScale = new Vector3(1);
            Trail.Position = position;
            Trail.Position.Z = -2;
            Trail.Rotation = new Vector3(0, 0, AngleFromVectors(position, Target));
            Rotation = Trail.Rotation;
            Velocity = SetVelocity(AngleFromVectors(position, Target), speed);
        }

        public void Deactivate()
        {
            Active = false;
            TrailTimer.Enabled = false;
            Trail.Active = false;
        }

        public bool Colusions()
        {
            if (TargetMod != null)
            {
                if (SphereIntersect2D(TargetMod))
                {
                    TargetMod.Active = false;
                    return true;
                }
            }

            return false;
        }
    }
}

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

    public class EnemyMissile : Mod
    {
        Mod Trail;
        Vector3 Target;

        public EnemyMissile(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            Trail = new Mod(game);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.1f, 1, 1);
            ModelScale = new Vector3(1);
            Radius = 1;

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
            Trail.DefuseColor = new Vector3(1, 0, 0);
            Trail.ModelScale = new Vector3(1.5f);

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                Trail.ModelScale = new Vector3(Vector3.Distance(Trail.Position, Position), 1.5f, 1);
            }

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Position = position;
            Active = true;
            Trail.Active = true;
            Trail.Position = position;
            Trail.Position.Z = -2;
            Target = new Vector3(Services.RandomMinMax(-480, 450), -450, 0);
            Trail.Rotation = new Vector3(0, 0, AngleFromVectors(position, Target));
            Rotation = Trail.Rotation;
            Velocity = SetVelocity(AngleFromVectors(position, Target), 10);
        }

    }
}

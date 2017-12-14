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
    using Mod = AModel;

    public class TargetedMissile : Mod
    {
        public Missile TheMissile;

        public TargetedMissile(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            TheMissile = new Missile(game, gameScale);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Moveable = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_Xmark");
        }

        public override void BeginRun()
        {
            Active = false;

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public void Deactivate()
        {
            Active = false;
            TheMissile.Deactivate();
        }

        public void Spawn(Vector3 basePos, Vector3 position)
        {
            Position = position;
            Active = true;

            TheMissile.Spawn(basePos, this, 300);
            TheMissile.TrailColor = new Vector3(0.1f, 0, 2);
            TheMissile.TimerAmount = 0.06f;
            DefuseColor = new Vector3(0, 0.1f, 2);
            MatrixUpdate();
            TheMissile.MatrixUpdate();
        }
    }
}

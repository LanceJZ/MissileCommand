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
    public class TargetedMissile : AModel
    {
        Missile TheMissiles;

        public Missile Missiles { get => TheMissiles; }

        public TargetedMissile(Game game) : base(game)
        {
            TheMissiles = new Missile(game);

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
            TheMissiles.Deactivate();
        }

        public void Spawn(Vector3 basePos, Vector3 position, Vector3 defuseColor)
        {
            Position = position;
            Active = true;

            TheMissiles.Spawn(basePos, this, 300, defuseColor);
            TheMissiles.TimerAmount = 0.06f;
            DefuseColor = defuseColor;
            MatrixUpdate();
            TheMissiles.MatrixUpdate();
        }
    }
}

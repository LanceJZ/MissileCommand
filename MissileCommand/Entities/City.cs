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
    public class City : AModel
    {
        Explosion Explode;
        SoundEffect ExplodeSound;

        public float TargetMin;
        public float TargetMax;

        public City(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            Explode = new Explosion(game, gameScale);
            Explode.Active = false;
        }

        public override void Initialize()
        {
            Moveable = false;

            base.Initialize();
            LoadContent();
            BeginRun();
        }

        public override void LoadContent()
        {
            LoadModel("MC_CityGS");
            ExplodeSound = LoadSoundEffect("Explosion");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public void Deactivate()
        {
            Active = false;
            ExplodeSound.Play();
            Explode.Spawn(Position);
            Explode.MaxSize = 3;
        }
    }
}

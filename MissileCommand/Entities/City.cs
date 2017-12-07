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

    public class City : Mod
    {


        public City(Game game) : base(game)
        {

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
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
    }
}

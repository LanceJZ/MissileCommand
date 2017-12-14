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


        public SmartBomb(Game game) : base(game)
        {

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Active = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_SmartBomb");
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

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

    public class Player : Mod
    {


        public Player(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue

            base.Initialize();
            LoadContent();
            BeginRun();
        }

        public override void LoadContent()
        {
            LoadModel("MC_CrossHair");
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

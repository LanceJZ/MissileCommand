using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;
using MissileCommand.Entities;

namespace MissileCommand
{
    public class GameLogic : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        Background TheBackground;

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game);

            // Screen resolution is 1200 X 900.
            game.Components.Add(this);
        }

        public override void Initialize()
        {

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {

        }

        public void BeginRun()
        {

        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
    }
}

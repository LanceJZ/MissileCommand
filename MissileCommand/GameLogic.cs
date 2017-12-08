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
        float GameScale = 1.91f;

        Background TheBackground;
        EnemyMissileController Missiles;

        Timer FPSTimer;
        float FPSFrames = 0;

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, GameScale);
            Missiles = new EnemyMissileController(game, GameScale, TheBackground);
            FPSTimer = new Timer(game, 1);

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
            FPSFrames++;

            if(FPSTimer.Expired)
            {
                FPSTimer.Reset();
                System.Diagnostics.Debug.WriteLine(FPSFrames.ToString());
                FPSFrames = 0;
            }

            base.Update(gameTime);
        }
    }
}

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
    using Mod = Engine.AModel;

    public class EnemyMissileController : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        List<EnemyMissile> Missiles;
        Background BackgroundRef;
        float GameScale;

        public EnemyMissileController(Game game, float gameScale, Background background) : base(game)
        {
            GameScale = gameScale;
            Missiles = new List<EnemyMissile>();
            BackgroundRef = background;

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
            for (int ii = 0; ii < 10; ii++)
            {
                float SpawnX = Services.RandomMinMax(-400, 400);

                for (int i = 0; i < 4; i++)
                {
                    Missiles.Add(new EnemyMissile(Game, GameScale));
                    Missiles.Last().Spawn(new Vector3(SpawnX, 450, 0));
                }
            }
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
    }
}

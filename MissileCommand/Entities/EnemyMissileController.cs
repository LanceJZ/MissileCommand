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
    public class EnemyMissileController : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        List<Missile> Missiles;
        Background BackgroundRef;
        Player PlayerRef;

        Timer FireTimer;
        float GameScale;
        int MaxNumberOfMissiles = 20;
        int Group = 4;

        public EnemyMissileController(Game game, float gameScale, Background background, Player player) : base(game)
        {
            GameScale = gameScale;
            Missiles = new List<Missile>();
            BackgroundRef = background;
            PlayerRef = player;
            FireTimer = new Timer(game, 1.2666f);


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
            if (FireTimer.Expired)
            {
                FireTimer.Reset();

                if (Missiles.Count < MaxNumberOfMissiles)
                {
                    FireMissile(Services.RandomMinMax(-400, 400));
                }
            }

            base.Update(gameTime);
        }

        void FireMissile(float spawnX)
        {
            Missiles.Add(new Missile(Game, GameScale));
            Missiles.Last().Spawn(new Vector3(spawnX, 450, 0));
            Missiles.Last().DefuseColor = new Vector3(0.1f, 1, 1);
            Missiles.Last().TrailColor = new Vector3(1, 0, 0);
        }
    }
}

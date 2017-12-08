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
    using Mod = Engine.AModel;

    public class Background : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        Mod[,] Missiles = new Mod[10, 3];
        Mod[] Ground = new Mod[5];
        City[] Cities = new City[6];

        float GameScale;

        public City[] TheCities
        {
            get => Cities;
        }

        public Background(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;

            for (int ii = 0; ii < 3; ii++)
            {
                for (int i = 0; i < 10; i++)
                {
                    Missiles[i, ii] = new Mod(game);
                }
            }

            for (int i = 0; i < Ground.Length; i++)
            {
                Ground[i] = new Mod(game);
            }


            for (int i = 0; i < Cities.Length; i++)
            {
                Cities[i] = new City(game);
            }

            // Screen resolution is 1200 X 900. Y positive on top of window. So up is positive.
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            float posX = -(253 * GameScale);

            foreach (Mod plot in Ground)
            {
                plot.ModelScale = new Vector3(GameScale);
                plot.Position.Z = -10;
                plot.Position.Y = (-Services.WindowHeight / 2);// + (14.5f * 1.9f);
                plot.Position.X = posX;
                plot.Moveable = false;
                plot.DefuseColor = new Vector3(1, 1, 0); //Yellow
                posX += (126 * GameScale);
            }

            foreach (City city in Cities)
            {
                city.ModelScale = new Vector3(GameScale);
                city.Position.Y = Ground[0].Position.Y + (12 * GameScale) + (14.5f * GameScale);
                city.DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue
            }

            foreach (Mod missile in Missiles)
            {
                missile.DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue
            }

            Cities[0].Position.X = -240 * GameScale;
            Cities[1].Position.X = -129 * GameScale;
            Cities[2].Position.X = -53 * GameScale;
            Cities[3].Position.X = 53 * GameScale;
            Cities[4].Position.X = 134 * GameScale;
            Cities[5].Position.X = 216 * GameScale;

            for (int i = 0; i < 3; i++)
            {
                int spot = 0;

                int bank = (i * 550) - 550;

                for (int line = 0; line < 4; line++)
                {
                    for (int row = 0; row < line + 1; row++)
                    {
                        float spaceX = 20;
                        Missiles[spot, i].Position.X = ((-spaceX * line) + (row * spaceX) + (line * (spaceX / 2))) + bank;
                        Missiles[spot, i].Position.Y = (-8 * line) -400;

                        spot++;
                    }
                }
            }

            base.Initialize();
            Services.AddBeginable(this);
            Services.AddLoadable(this);
        }

        public void BeginRun()
        {

        }

        public void LoadContent()
        {
            for (int i = 0; i < 5; i++)
            {
                Ground[i].LoadModel("MC_Ground-" + i.ToString());
            }

            foreach(Mod missile in Missiles)
            {
                missile.LoadModel("MC_MissileAmmo");
            }
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }
    }
}

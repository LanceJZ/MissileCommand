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
    public class Background : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        AModel[] Ground = new AModel[5];
        City[] TheCities = new City[6];

        MissileBase[] BaseLocations = new MissileBase[3];

        float GameScale;

        public City[] Cities { get => TheCities; }
        public MissileBase[] Bases { get => BaseLocations; }

        public Background(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;

            for (int ii = 0; ii < 3; ii++)
            {
                BaseLocations[ii] = new MissileBase(game, gameScale);
            }

            for (int i = 0; i < Ground.Length; i++)
            {
                Ground[i] = new AModel(game);
            }


            for (int i = 0; i < TheCities.Length; i++)
            {
                Cities[i] = new City(game, GameScale);
            }

            // Screen resolution is 1200 X 900. Y positive on top of window. So up is positive.
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            float posX = -(253 * GameScale);

            foreach (AModel plot in Ground)
            {
                plot.ModelScale = new Vector3(GameScale);
                plot.Position.Z = -10;
                plot.Position.Y = (-Services.WindowHeight / 2);// + (14.5f * 1.9f);
                plot.Position.X = posX;
                plot.Moveable = false;
                plot.DefuseColor = new Vector3(1, 1, 0); //Yellow
                posX += (126 * GameScale);
            }

            foreach (City city in TheCities)
            {
                city.ModelScale = new Vector3(GameScale);
                city.Position.Y = Ground[0].Position.Y + (12 * GameScale) + (14.5f * GameScale);
                city.DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue
            }

            TheCities[0].Position.X = -240 * GameScale;
            TheCities[1].Position.X = -129 * GameScale;
            TheCities[2].Position.X = -53 * GameScale;
            TheCities[3].Position.X = 53 * GameScale;
            TheCities[4].Position.X = 134 * GameScale;
            TheCities[5].Position.X = 216 * GameScale;

            for (int i = 0; i < 3; i++)
            {
                int bank = (i * 550) - 550;
                BaseLocations[i].Spawn(new Vector3(0.2f, 0.1f, 2.5f));
                BaseLocations[i].Setup(new Vector3(bank, -400, 0));
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
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public void IsCityHit()
        {
            foreach(City city in TheCities)
            {

            }
        }
    }
}

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
        GameLogic GameLogicRef;
        AModel[] Ground = new AModel[5];
        City[] TheCities = new City[6];
        MissileBase[] BaseLocations = new MissileBase[3];

        public City[] Cities { get => TheCities; }
        public MissileBase[] Bases { get => BaseLocations; }

        public Background(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;

            for (int ii = 0; ii < 3; ii++)
            {
                BaseLocations[ii] = new MissileBase(game);
            }

            for (int i = 0; i < Ground.Length; i++)
            {
                Ground[i] = new AModel(game);
            }


            for (int i = 0; i < TheCities.Length; i++)
            {
                Cities[i] = new City(game, GameLogicRef.GameScale);
            }

            game.Components.Add(this);
            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place things behind, they are in the negative.
        }

        public override void Initialize()
        {
            float posX = -(253 * GameLogicRef.GameScale);

            foreach (AModel plot in Ground)
            {
                plot.ModelScale = new Vector3(GameLogicRef.GameScale);
                plot.Position.Z = -10;
                plot.Position.Y = (-Services.WindowHeight / 2);
                plot.Position.X = posX;
                plot.Moveable = false;
                plot.DefuseColor = new Vector3(1, 1, 0); //Yellow
                posX += (126 * GameLogicRef.GameScale);
            }

            foreach (City city in TheCities)
            {
                city.ModelScale = new Vector3(GameLogicRef.GameScale);
                city.Position.Y = Ground[0].Position.Y + (12 * GameLogicRef.GameScale)
                    + (14f * GameLogicRef.GameScale);
                city.DefuseColor = new Vector3(0.2f, 0.1f, 2.8f); // Reddish Blue
            }

            TheCities[0].Position.X = -241 * GameLogicRef.GameScale;
            TheCities[1].Position.X = -130 * GameLogicRef.GameScale;
            TheCities[2].Position.X = -54 * GameLogicRef.GameScale;
            TheCities[3].Position.X = 52 * GameLogicRef.GameScale;
            TheCities[4].Position.X = 132 * GameLogicRef.GameScale;
            TheCities[5].Position.X = 214 * GameLogicRef.GameScale;

            foreach (City city in TheCities)
            {
                city.TargetMin = city.Position.X - 20;
                city.TargetMax = city.Position.X + 20;
            }

            //Land files are in this order. 0 = 2, 1 = 0, 2 = 4, 3, 4 = 1.

            for (int i = 0; i < 3; i++)
            {
                int bank = (i * 550) - 550;
                BaseLocations[i].Setup(new Vector3(bank, -400, 0));
            }

            base.Initialize();
            Services.AddBeginable(this);
            Services.AddLoadable(this);
        }

        public void BeginRun()
        {
            GameOver();
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

        public void NewWave(Vector3 defuseColor)
        {
            foreach (MissileBase silo in Bases)
                silo.Spawn(defuseColor);
        }

        public void GameOver()
        {
            foreach (City city in Cities)
            {
                city.Active = false;
            }

            foreach (MissileBase silo in Bases)
            {
                silo.Deativate();
            }
        }

        public void NewGame()
        {
            NewWave(new Vector3(0.2f, 0.1f, 2.8f));

            foreach(City city in Cities)
            {
                city.Active = true;
            }
        }
    }
}

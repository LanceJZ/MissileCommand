using Microsoft.Xna.Framework;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;
using Engine;

namespace MissileCommand.Entities
{
    public class MissileBase : PositionedObject
    {
        Explosion Explode;
        AModel[] TheMissiles = new AModel[10];

        public AModel[] Missiles { get => TheMissiles; }


        public MissileBase(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;

            Explode = new Explosion(game, gameScale);
            Explode.Active = false;

            for (int i = 0; i < 10; i++)
            {
                TheMissiles[i] = new AModel(game);
            }

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Radius = 10;

            base.Initialize();
        }

        public void LoadContent()
        {
            foreach (AModel missile in TheMissiles)
            {
                missile.LoadModel("MC_MissileAmmo");
            }
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public bool MissileFired()
        {
            int lastInStack = 0;

            foreach (AModel missile in TheMissiles)
            {
                if (missile.Active)
                    lastInStack++;

                if (lastInStack < 1)
                    return false;
            }

            TheMissiles[lastInStack - 1].Active = false;

            return true;
        }

        public void Setup(Vector3 position)
        {
            Position = position;
            int spot = 0;

            for (int line = 0; line < 4; line++)
            {
                for (int row = 0; row < line + 1; row++)
                {
                    float spaceX = 20;
                    TheMissiles[spot].Position.X = ((-spaceX * line) + (row * spaceX) + (line * (spaceX / 2))) + Position.X;
                    TheMissiles[spot].Position.Y = (-8 * line) - 400;
                    spot++;
                }
            }
        }

        public void Spawn(Vector3 color)
        {
            Active = true;

            foreach (AModel missile in TheMissiles)
            {
                missile.DefuseColor = color;
                missile.Active = true;
            }
        }

        public void Deativate()
        {
            Active = false;

            Explode.Spawn(Position);
            Explode.MaxSize = 3;

            foreach (AModel missile in TheMissiles)
            {
                missile.Active = false;
            }
        }
    }
}

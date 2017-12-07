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
    using Mod = AModel;

    public class EnemyMissile : Mod
    {
        List<Mod> Trail;
        XnaModel TrailModel;
        Vector3 TrailColor;

        public EnemyMissile(Game game) : base(game)
        {
            Trail = new List<Mod>();
        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.1f, 1, 1);
            TrailColor = new Vector3(2, 0, 0);
            Scale = 2;

            base.Initialize();
            LoadContent();
            BeginRun();
        }

        public override void LoadContent()
        {
            LoadModel("Cube");
            TrailModel = Load("Cube");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Position = position;
            Active = true;


        }

        void MakeNewTrailCube()
        {
            bool spawnNew = true;
            int freeOne = Trail.Count;

            for (int i = 0; i < Trail.Count; i++)
            {
                if (!Trail[i].Active)
                {
                    spawnNew = false;
                    freeOne = i;
                    break;
                }

                if (spawnNew)
                {
                    Trail.Add(new Mod(Game));
                    Trail.Last().SetModel(TrailModel);
                    Trail.Last().Moveable = false;
                }

                Trail[freeOne].Active = true;
                Trail[freeOne].Position = Position;
            }
        }
    }
}

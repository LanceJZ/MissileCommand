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

    public class Explosion : Mod
    {
        Timer BlinkTimer;
        float Speed = 0.5f;
        float TheMaxSize = 1.5f;
        bool Growing = true;
        bool Blinked = true;

        public float MaxSize { set => TheMaxSize = value; }

        public Explosion(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            BlinkTimer = new Timer(game, 0.1f);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Moveable = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_Explode");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (BlinkTimer.Expired)
            {
                BlinkTimer.Reset();

                if (Blinked)
                {
                    Blinked = false;
                    DefuseColor = new Vector3(2, 0, 1);
                }
                else
                {
                    Blinked = true;
                    DefuseColor = new Vector3(1, 0, 2);
                }
            }

            if (Active)
            {
                Radius = SphereRadius * ModelScale.X;

                if (Growing)
                {
                    ModelScaleVelocity = new Vector3(Speed * 2, Speed * 2, 0);

                    if (ModelScale.X > TheMaxSize)
                    {
                        Growing = false;
                    }
                }
                else
                {
                    ModelScaleVelocity = new Vector3(-Speed, -Speed, 0);

                    if (ModelScale.X < 0)
                    {
                        Active = false;
                    }
                }
            }

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Position = position;
            Position.Z = 10;
            Active = true;
            BlinkTimer.Reset();
            ModelScale = new Vector3(0, 0, 1);
            Growing = true;
        }
    }
}

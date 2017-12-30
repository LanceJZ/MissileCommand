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
    public class Explosion : AModel
    {
        GameLogic GameLogicRef;
        Timer BlinkTimer;
        float Speed = 0.5f;
        float TheMaxSize = 1.5f;
        int CurrentColor= 0;
        bool Growing = true;
        bool GrowingSet;
        bool ShrinkingSet;

        public float MaxSize { set => TheMaxSize = value; }

        public Explosion(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;
            BlinkTimer = new Timer(game, 0.0666f);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Moveable = false;
            Active = false;

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

                CurrentColor++;

                if (CurrentColor > 4)
                    CurrentColor = 0;

                DefuseColor = GameLogicRef.LevelColors[CurrentColor];
            }

            if (Active)
            {
                Radius = SphereRadius * ModelScale.X;

                if (Growing)
                {
                    if (!GrowingSet)
                    {
                        ModelScaleVelocity = new Vector3(Speed * 2);
                        GrowingSet = true;
                    }

                    if (ModelScale.X > TheMaxSize)
                    {
                        Growing = false;
                    }
                }
                else
                {
                    if (!ShrinkingSet)
                    {
                        ModelScaleVelocity = new Vector3(-Speed);
                        ShrinkingSet = true;
                    }

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
            ModelScale = new Vector3(0.1f);
            Growing = true;
            GrowingSet = false;
            ShrinkingSet = false;
            MatrixUpdate();
        }
    }
}

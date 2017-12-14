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
    public class Bomber : AModel
    {
        GameLogic GameLogicRef;

        Timer DropBombTimer;
        Timer AnimationTimer;

        bool AnimationDone;
        bool AnimationStart;

        public Timer DropTimer { get => DropBombTimer; }

        public Bomber(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameLogicRef = gameLogic;
            GameScale = gameScale;

            DropBombTimer = new Timer(game);
            AnimationTimer = new Timer(game);

            LoadContent();
            BeginRun();
            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place things behind, they are in the negative.
        }

        public override void Initialize()
        {
            Active = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_Bomber");
        }

        public override void BeginRun()
        {
            Radius = SphereRadius;

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {
                if (Hit)
                {
                    RunEndAnimation();
                    return;
                }

                if (Position.X > 650 || Position.X < -650)
                {
                    Active = false;
                    GameLogicRef.BomberTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                    return;
                }

                if (DropBombTimer.Expired)
                {
                    DropBombTimer.Reset(Services.RandomMinMax(20.0f, 30.0f));
                    GameLogicRef.DropBombs(Position, 4);
                }

                GameLogicRef.CheckCollusion(this, GameLogicRef.BomberTimer);
            }
        }

        public void Spawn()
        {
            if (Position.X < 0)
            {
                Rotation.Y = 0;
            }
            else
            {
                Rotation.Y = MathHelper.Pi;
            }

            Rotation.X = 0;
            RotationAcceleration.X = 0;
            RotationVelocity.X = 0;
            AnimationDone = false;
            AnimationStart = false;
            MatrixUpdate();
        }

        void RunEndAnimation()
        {
            if (!AnimationStart)
            {
                RotationAcceleration.X = Services.RandomMinMax(-1.0f, 1.0f);
                AnimationStart = true;
                AnimationTimer.Reset(Services.RandomMinMax(2, 4));
            }

            if (AnimationTimer.Expired)
            {
                Active = false;
            }
        }
    }
}

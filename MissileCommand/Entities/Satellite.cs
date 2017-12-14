﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;

namespace MissileCommand.Entities
{
    public class Satellite : AModel
    {
        GameLogic GameLogicRef;

        Timer DropBombTimer;

        public Timer DropTimer { get => DropBombTimer; }

        public Satellite(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameLogicRef = gameLogic;
            GameScale = gameScale;

            DropBombTimer = new Timer(game);

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            Active = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_Satalite");
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
                    Position.X = 700;
                    base.Update(gameTime);
                    Active = false;
                    return;
                }

                if (Position.X > 650 || Position.X < -650)
                {
                    Hit = true;
                    GameLogicRef.SatatliteTimer.Reset(Services.RandomMinMax(10.0f, 20.0f));
                    Position.X = 700;
                    return;
                }

                if (DropBombTimer.Expired)
                {
                    DropBombTimer.Reset(Services.RandomMinMax(20.0f, 30.0f));
                    GameLogicRef.DropBombs(Position, 2);
                }

                GameLogicRef.CheckCollusion(this, GameLogicRef.SatatliteTimer);
            }
        }

        public void Spawn()
        {
            RotationVelocity.Y = MathHelper.PiOver4;
        }
    }
}
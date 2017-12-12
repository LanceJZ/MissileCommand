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

        public Bomber(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameLogicRef = gameLogic;
            GameScale = gameScale;

            DropBombTimer = new Timer(game);

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

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                if (Position.X > 600 || Position.X < -600)
                {
                    Active = false;
                    GameLogicRef.BomberTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                    return;
                }

                if (DropBombTimer.Expired)
                {
                    DropBombTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                    DropBombs();
                }
            }

            base.Update(gameTime);
        }

        public void Spawn(Vector3 position)
        {
            Position = position;

            if (Position.X < 0)
            {
                Rotation.Y = 0;
                Velocity.X = 20;
            }
            else
            {
                Rotation.Y = MathHelper.Pi;
                Velocity.X = -20;
            }

            DropBombTimer.Reset(Services.RandomMinMax(2.0f, 15.0f));

            Active = true;
        }

        public void CheckCollusion(Explosion explosion)
        {
            if (SphereIntersect2D(explosion))
            {
                Active = false;
                GameLogicRef.ScoreUpdate(10);
                GameLogicRef.PlayerRef.SetExplode(Position);
                GameLogicRef.BomberTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
            }
        }

        void DropBombs()
        {
            EnemyMissileController eMC = GameLogicRef.MissilesRef;

            for (int i = 0; i < 4; i++)
            {
                Vector3 adjust = new Vector3(0, -10, 0) + Position;

                eMC.FireMissile(adjust, eMC.ChoseTarget(), eMC.MissileSpeed);
            }
        }
    }
}

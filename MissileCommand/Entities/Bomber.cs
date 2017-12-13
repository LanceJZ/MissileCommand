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

        public Timer DropTimer { get => DropBombTimer; }

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
                    Active = false;
                    return;
                }

                if (Position.X > 650 || Position.X < -650)
                {
                    Hit = true;
                    Position.X = 700;
                    GameLogicRef.BomberTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                    return;
                }

                if (DropBombTimer.Expired)
                {
                    DropBombTimer.Reset(Services.RandomMinMax(20.0f, 30.0f));
                    GameLogicRef.DropBombs(Position, 4);
                }

                foreach (Explosion explode in GameLogicRef.PlayerRef.Explosions)
                {
                    if (explode.Active)
                    {
                        if (CheckCollusion(explode))
                            Hit = true;

                        break;
                    }
                }
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
        }

        public bool CheckCollusion(Explosion explosion) //TODO: Does not always detect hit.
        {
            if (CirclesIntersect(explosion))
            {
                GameLogicRef.ScoreUpdate(100);
                GameLogicRef.PlayerRef.SetExplode(Position);
                GameLogicRef.BomberTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                Position.X = 700;
                return true;
            }

            return false;
        }

        void DropBombs()
        {
            EnemyMissileController enemyMC = GameLogicRef.MissilesRef;

            for (int i = 0; i < 4; i++)
            {
                Vector3 adjust = new Vector3(0, -10, 0) + Position;

                enemyMC.FireMissile(adjust, enemyMC.ChoseTarget(), enemyMC.MissileSpeed);
            }
        }
    }
}

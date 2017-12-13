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
                    GameLogicRef.DropBombs(Position, 2);
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
            RotationVelocity.Y = MathHelper.PiOver4;
        }

        public bool CheckCollusion(Explosion explosion)
        {
            if (SphereIntersect2D(explosion))
            {
                Position.X = 700;
                GameLogicRef.ScoreUpdate(100);
                GameLogicRef.PlayerRef.SetExplode(Position);
                GameLogicRef.SatatliteTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                return true;
            }

            return false;
        }

        void DropBombs()
        {
            EnemyMissileController enemyMC = GameLogicRef.MissilesRef;

            for (int i = 0; i < 2; i++)
            {
                Vector3 adjust = new Vector3(0, -10, 0) + Position;

                enemyMC.FireMissile(adjust, enemyMC.ChoseTarget(), enemyMC.MissileSpeed);
            }
        }
    }
}

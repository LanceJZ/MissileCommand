using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;
using MissileCommand.Entities;

namespace MissileCommand
{
    public class GameLogic : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        float GameScale = 1.91f;

        Background TheBackground;
        EnemyMissileController TheMissiles;
        Player ThePlayer;
        Bomber TheBomber;
        Satellite TheSatalite;

        Numbers TheScoreDisplay;

        Timer FPSTimer;
        Timer BomberRunTimer;
        Timer SataliteRunTimer;

        float FPSFrames = 0;

        float Score;

        public Background BackgroundRef { get => TheBackground; }
        public Player PlayerRef { get => ThePlayer; }
        public EnemyMissileController MissilesRef { get => TheMissiles; }
        public Bomber BomberRef { get => TheBomber; }
        public Timer BomberTimer { get => BomberRunTimer; }
        public Timer SatatliteTimer { get => SataliteRunTimer; }

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, GameScale);
            ThePlayer = new Player(game, this, GameScale);
            TheMissiles = new EnemyMissileController(game, this, GameScale);
            TheBomber = new Bomber(game, this, GameScale);
            TheSatalite = new Satellite(game, this, GameScale);

            FPSTimer = new Timer(game, 1);
            BomberRunTimer = new Timer(game);
            SataliteRunTimer = new Timer(game);

            TheScoreDisplay = new Numbers(game);

            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place objects behind other objects, put them in the negative.
            game.Components.Add(this);
        }

        public override void Initialize()
        {

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {

        }

        public void BeginRun()
        {

        }

        public override void Update(GameTime gameTime)
        {
            FPSFrames++;

            if(FPSTimer.Expired)
            {
                FPSTimer.Reset();
                System.Diagnostics.Debug.WriteLine(FPSFrames.ToString());
                FPSFrames = 0;
            }

            if (BomberRunTimer.Expired && TheMissiles.Wave > 0)
            {
                BomberRunTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));

                if (Services.RandomMinMax(0, 100) > 25)
                {
                    if (!TheBomber.Active)
                        BomberRun();
                }
            }

            if (SataliteRunTimer.Expired && TheMissiles.Wave > 1)
            {
                SataliteRunTimer.Reset(Services.RandomMinMax(10.0f, 20.0f));

                if (Services.RandomMinMax(0, 100) > 25)
                {
                    if (!TheSatalite.Active)
                        SataliteRun();
                }
            }

            base.Update(gameTime);
        }

        public void ScoreUpdate(int score)
        {
            int muliplier = (int)(TheMissiles.Wave / 2.1f) + 1;

            Score += (score * muliplier);

            System.Diagnostics.Debug.WriteLine("Score: " + Score.ToString());
        }

        void BomberRun()
        {
            float spawnX;

            if (Services.RandomMinMax(0, 100) > 50)
            {
                spawnX = -600;
            }
            else
            {
                spawnX = 600;
            }

            Spawn(new Vector3(spawnX, Services.RandomMinMax(50.0f, 250.0f), 0), TheBomber, TheBomber.DropTimer);
            TheBomber.Spawn();
        }

        void SataliteRun()
        {
            float spawnX;

            if (Services.RandomMinMax(0, 100) > 50)
            {
                spawnX = -600;
            }
            else
            {
                spawnX = 600;
            }

            Spawn(new Vector3(spawnX, Services.RandomMinMax(200.0f, 400.0f), 0), TheSatalite, TheSatalite.DropTimer);
            TheSatalite.Spawn();
        }

        public void Spawn(Vector3 position, PositionedObject po, Timer timer)
        {
            po.Position = position;
            timer.Reset(Services.RandomMinMax(10.0f, 35.0f));
            po.Active = true;
            po.Hit = false;

            if (po.Position.X < 0)
            {
                po.Velocity.X = 20;
            }
            else
            {
                po.Velocity.X = -20;
            }

        }

        public void CheckCollusion(AModel model, PositionedObject po, Explosion explosion, Timer timer)
        {
            if (model.SphereIntersect2D(explosion))
            {
                po.Hit = true;
                po.Position.X = 700;
                ScoreUpdate(100);
                PlayerRef.SetExplode(po.Position);
                timer.Reset(Services.RandomMinMax(10.0f, 30.0f));
            }
        }

        public void DropBombs(Vector3 position, int numberOfMissiles)
        {
            for (int i = 0; i < numberOfMissiles; i++)
            {
                Vector3 adjust = new Vector3(0, -10, 0) + position;

                TheMissiles.FireMissile(adjust, TheMissiles.ChoseTarget(), TheMissiles.MissileSpeed);
            }
        }

    }
}

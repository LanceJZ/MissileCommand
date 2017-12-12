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

        Numbers TheScoreDisplay;

        Timer FPSTimer;
        Timer BomberRunTimer;

        float FPSFrames = 0;

        float Score;

        public Background BackgroundRef { get => TheBackground; }
        public Player PlayerRef { get => ThePlayer; }
        public EnemyMissileController MissilesRef { get => TheMissiles; }
        public Bomber BomberRef { get => TheBomber; }
        public Timer BomberTimer { get => BomberRunTimer; }

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, GameScale);
            ThePlayer = new Player(game, this, GameScale);
            TheMissiles = new EnemyMissileController(game, this, GameScale);
            TheBomber = new Bomber(game, this, GameScale);

            FPSTimer = new Timer(game, 1);
            BomberRunTimer = new Timer(game);

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

            if (BomberRunTimer.Expired && TheMissiles.Wave > 2)
            {
                BomberRunTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));

                if (!TheBomber.Active)
                    BomberRun();
            }

            base.Update(gameTime);
        }

        public void ScoreUpdate(int score)
        {
            Score += score;
            System.Diagnostics.Debug.WriteLine("Score: " + Score.ToString());
        }

        void BomberRun()
        {
            if (Services.RandomMinMax(0, 100) > 50)
            {
                TheBomber.Spawn(new Vector3(-600, Services.RandomMinMax(100.0f, 300.0f), 0));
            }
            else
            {
                TheBomber.Spawn(new Vector3(600, Services.RandomMinMax(100.0f, 300.0f), 0));
            }
        }
    }
}

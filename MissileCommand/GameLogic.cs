﻿using Microsoft.Xna.Framework;
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
        SmartBomb TheSmartBomb;

        Numbers TheScoreDisplay;
        Words TheScoreText;

        Timer FPSTimer;
        Timer BomberRunTimer;
        Timer SataliteRunTimer;
        Timer SmartBombRunTimer;
        Timer RadarSoundTimer;

        SoundEffect RadarSound;

        Vector3 TheEnemyColor = new Vector3(1, 0, 0);
        Vector3 ThePlayerColor = new Vector3(0.2f, 0.1f, 2.5f);

        float FPSFrames = 0;

        int Score;

        public Background BackgroundRef { get => TheBackground; }
        public Player PlayerRef { get => ThePlayer; }
        public EnemyMissileController MissilesRef { get => TheMissiles; }
        public Bomber BomberRef { get => TheBomber; }
        public Timer BomberTimer { get => BomberRunTimer; }
        public Timer SatatliteTimer { get => SataliteRunTimer; }
        public Timer SmartBombTimer { get => SmartBombRunTimer; }

        public GameLogic(Game game) : base(game)
        {
            TheBackground = new Background(game, GameScale);
            ThePlayer = new Player(game, this, GameScale);
            TheMissiles = new EnemyMissileController(game, this, GameScale);
            TheBomber = new Bomber(game, this, GameScale);
            TheSatalite = new Satellite(game, this, GameScale);
            TheSmartBomb = new SmartBomb(game, this, GameScale);

            FPSTimer = new Timer(game, 1);
            BomberRunTimer = new Timer(game);
            SataliteRunTimer = new Timer(game);
            SmartBombRunTimer = new Timer(game);
            RadarSoundTimer = new Timer(game);

            TheScoreDisplay = new Numbers(game);
            TheScoreText = new Words(game);

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
            RadarSound = PlayerRef.LoadSoundEffect("Radar");
        }

        public void BeginRun()
        {
            TheScoreDisplay.ProcessNumber(0, new Vector3(-250, 400, 100), 2);
            TheScoreText.ProcessWords("SCORE", new Vector3(-550, 400, 100), 2);

            RadarSoundTimer.Amount = RadarSound.Duration.Seconds;
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

            if (MissilesRef.MissilesLaunched < MissilesRef.MaxMissiles)
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

                if (BomberRunTimer.Expired && TheMissiles.Wave > 0)
                {
                    BomberRunTimer.Reset(Services.RandomMinMax(10.0f, 30.0f));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheBomber.Active)
                            BomberRun(spawnX);
                    }
                }

                if (SataliteRunTimer.Expired && TheMissiles.Wave > 1)
                {
                    SataliteRunTimer.Reset(Services.RandomMinMax(10.0f, 20.0f));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheSatalite.Active)
                            SataliteRun(spawnX);
                    }
                }

                if (SmartBombRunTimer.Expired && TheMissiles.Wave > 5)
                {
                    SmartBombRunTimer.Reset(Services.RandomMinMax(10, 20));

                    if (Services.RandomMinMax(0, 100) > 25)
                    {
                        if (!TheSmartBomb.Active)
                        {
                            TheSmartBomb.Spawn(new Vector3(Services.RandomMinMax(-400, 400), 550, 0));
                            TheSmartBomb.DefuseColor = TheEnemyColor;
                        }
                    }
                }

                if (TheBomber.Active || TheSatalite.Active)
                {
                    if (RadarSoundTimer.Expired)
                    {
                        RadarSoundTimer.Reset();
                        RadarSound.Play();
                    }
                }
            }

            base.Update(gameTime);
        }

        public void ScoreUpdate(int score)
        {
            int muliplier = (int)(TheMissiles.Wave / 2.1f) + 1;

            Score += (score * muliplier);

            TheScoreDisplay.UpdateNumber(Score);
        }

        void BomberRun(float spawnX)
        {
            Spawn(new Vector3(spawnX, Services.RandomMinMax(50.0f, 250.0f), 0), TheBomber, TheBomber.DropTimer);
            TheBomber.Spawn();
            TheBomber.DefuseColor = new Vector3(1, 0, 0);
        }

        void SataliteRun(float spawnX)
        {
            Spawn(new Vector3(spawnX, Services.RandomMinMax(300.0f, 400.0f), 0), TheSatalite, TheSatalite.DropTimer);
            TheSatalite.Spawn();
            TheSatalite.DefuseColor = new Vector3(1, 0, 0);
        }

        public void Spawn(Vector3 position, PositionedObject po, Timer timer)
        {
            timer.Reset(Services.RandomMinMax(10.0f, 35.0f));
            po.Position = position;
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

        public void CheckCollusion(PositionedObject po, Timer timer)
        {
            foreach (Explosion explode in PlayerRef.Explosions)
            {
                if (explode.Active)
                {
                    if (po.CirclesIntersect(explode))
                    {
                        ScoreUpdate(100);
                        PlayerRef.SetExplode(po.Position);
                        timer.Reset(Services.RandomMinMax(10.0f, 30.0f));
                        po.Hit = true;
                        break;
                    }
                }
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

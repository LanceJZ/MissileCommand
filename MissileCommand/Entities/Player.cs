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
    enum BaseFired
    {
        Alpha,
        Delta,
        Ogega
    };

    public class Player : AModel
    {
        Background BackgroundRef;
        GameLogic GameLogicRef;
        List<TargetedMissile> TheMissiles;
        List<Explosion> TheExplosions;

        SoundEffect LaunchSound;
        SoundEffect ExplosionSound;
        SoundEffect EmptySiloSound;

        MouseState LastMouseState;
        KeyboardState LastKeyState;

        Timer MouseReadTimer;

        float MoveSpeed = 400;

        public List<Explosion> Explosions { get => TheExplosions; }

        public Player(Game game, GameLogic gameLogic) : base(game)
        {
            BackgroundRef = gameLogic.BackgroundRef;
            GameLogicRef = gameLogic;
            TheMissiles = new List<TargetedMissile>();
            TheExplosions = new List<Explosion>();
            MouseReadTimer = new Timer(game, 0.0666f);

            LoadContent();
            BeginRun();
            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place things behind, they are in the negative.
        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue
            Position.Z = 20;
            Active = false;

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_CrossHair");
            LaunchSound = LoadSoundEffect("Player Missile Launch");
            ExplosionSound = LoadSoundEffect("Explosion");
            EmptySiloSound = LoadSoundEffect("Empty Silo");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            if (GameLogicRef.CurrentMode == GameState.InPlay)
            {
                GetInput();
                MissileCollusion();
            }

            base.Update(gameTime);
        }

        public void NewWave()
        {
            foreach(TargetedMissile missile in TheMissiles)
            {
                if (missile.Active)
                    missile.Deactivate();
            }

            Position.X = 0;
            Position.Y = 0;
        }

        public void NewGame()
        {
            Position.X = 0;
            Position.Y = 0;
            Active = true;
        }

        public void GameOver()
        {
            Active = false;
        }

        void MissileCollusion()
        {
            foreach (TargetedMissile missile in TheMissiles)
            {
                if (missile.Active)
                {
                    if (missile.SphereIntersect2D(missile.Missiles))
                    {
                        missile.Deactivate();
                        SetExplode(missile.Position);
                        break;
                    }
                }
            }
        }

        void GetInput()
        {
            MouseState theMouse = Mouse.GetState();
            KeyboardState theKeyboard = Keyboard.GetState();

            if (theKeyboard.IsKeyDown(Keys.Up))
            {
                MoveUp();

            }
            else if (theKeyboard.IsKeyDown(Keys.Down))
            {
                MoveDown();

            }
            else if (theKeyboard.IsKeyDown(Keys.Left))
            {
                MoveLeft();

            }
            else if (theKeyboard.IsKeyDown(Keys.Right))
            {
                MoveRight();
            }
            else
                StopMove();

            int centerX = Services.WindowWidth / 2;
            int centerY = Services.WindowHeight / 2;

            MouseState mouseState = Mouse.GetState();

            if (mouseState.X > centerX)
                MoveRight();
            else if (mouseState.X < centerX)
                MoveLeft();

            if (mouseState.Y > centerY)
                MoveDown();
            else if (mouseState.Y < centerY)
                MoveUp();

            if (MouseReadTimer.Expired)
            {
                MouseReadTimer.Reset();
                Mouse.SetPosition(centerX, centerY);
            }

            if (!LastKeyState.IsKeyDown(Keys.Z))
            {
                if (theKeyboard.IsKeyDown(Keys.Z))
                {
                    if (BackgroundRef.Bases[0].MissileFired())
                        FireMissile(new Vector3(-550, -400, 0));
                    else
                        EmptySiloSound.Play();
                }
            }

            if (!LastKeyState.IsKeyDown(Keys.X))
            {
                if (theKeyboard.IsKeyDown(Keys.X))
                {
                    if (BackgroundRef.Bases[1].MissileFired())
                        FireMissile(new Vector3(0, -400, 0));
                    else
                        EmptySiloSound.Play();
                }
            }

            if (!LastKeyState.IsKeyDown(Keys.C))
            {
                if (theKeyboard.IsKeyDown(Keys.C))
                {
                    if (BackgroundRef.Bases[2].MissileFired())
                        FireMissile(new Vector3(550, -400, 0));
                    else
                        EmptySiloSound.Play();
                }
            }

            LastMouseState = theMouse;
            LastKeyState = theKeyboard;
        }

        void FireMissile(Vector3 basePos)
        {
            LaunchSound.Play();

            bool spawnNew = true;
            int freeOne = TheMissiles.Count;

            for (int i = 0; i < TheMissiles.Count; i ++)
            {
                if (!TheMissiles[i].Active)
                {
                    spawnNew = false;
                    freeOne = i;
                    break;
                }
            }

            if (spawnNew)
            {
                TheMissiles.Add(new TargetedMissile(Game));
            }

            TheMissiles[freeOne].Spawn(basePos, Position, DefuseColor);
        }

        public void SetExplode(Vector3 position)
        {
            ExplosionSound.Play();

            bool spawnNew = true;
            int freeOne = TheExplosions.Count;

            for (int i = 0; i < TheExplosions.Count; i++)
            {
                if (!TheExplosions[i].Active)
                {
                    spawnNew = false;
                    freeOne = i;
                    break;
                }
            }

            if (spawnNew)
            {
                TheExplosions.Add(new Explosion(Game, GameLogicRef));
            }

            TheExplosions[freeOne].Spawn(position);
        }

        void MoveUp()
        {
            if (Position.Y < Services.WindowHeight * 0.5f - 50)
            {
                Velocity.Y = MoveSpeed;
            }
            else
            {
                Velocity.Y = 0;
            }
        }

        void MoveDown()
        {
            if (Position.Y > -Services.WindowHeight * 0.5f + 100)
            {
                Velocity.Y = -MoveSpeed;
            }
            else
            {
                Velocity.Y = 0;
            }
        }

        void MoveRight()
        {
            if (Position.X < Services.WindowWidth * 0.5f - 50)
            {
                Velocity.X = MoveSpeed;
            }
            else
            {
                Velocity.X = 0;
            }
        }

        void MoveLeft()
        {
            if (Position.X > -Services.WindowWidth * 0.5f + 50)
            {
                Velocity.X = -MoveSpeed;
            }
            else
            {
                Velocity.X = 0;
            }
        }

        void StopMove()
        {
            Velocity = Vector3.Zero;
        }
    }
}

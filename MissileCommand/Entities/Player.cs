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
    using Mod = AModel;

    enum BaseFired
    {
        Alpha,
        Delta,
        Ogega
    };

    public class Player : Mod
    {
        List<TargetedMissile> TheMissiles;
        public List<Explosion> TheExplosions;
        MouseState LastMouseState;
        KeyboardState LastKeyState;

        float MoveSpeed = 100;

        public Player(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            TheMissiles = new List<TargetedMissile>();
            TheExplosions = new List<Explosion>();

            LoadContent();
            BeginRun();
            // Screen resolution is 1200 X 900. Y positive on top of window. So up is positive.
        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_CrossHair");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            GetInput();
            MissileCollusion();

            base.Update(gameTime);
        }

        void MissileCollusion()
        {
            foreach (TargetedMissile tMissile in TheMissiles)
            {
                if (tMissile.Active)
                {
                    if (tMissile.SphereIntersect2D(tMissile.TheMissile))
                    {
                        tMissile.Deactivate();
                        SetExplode(tMissile.Position);
                        break;
                    }
                }
            }
        }

        // Screen resolution is 1200 X 900.
        // Y positive on top of window. So down is negative.
        // X positive is left of window. So to the right is negative.
        void GetInput()
        {
            MouseState theMouse = Mouse.GetState();
            KeyboardState theKeyboard = Keyboard.GetState();

            if (theKeyboard.IsKeyDown(Keys.Up))
                MoveUp();
            else if (theKeyboard.IsKeyDown(Keys.Down))
                MoveDown();
            else if (theKeyboard.IsKeyDown(Keys.Left))
                MoveLeft();
            else if (theKeyboard.IsKeyDown(Keys.Right))
                MoveRight();
            else
                StopMove();

            if (!LastKeyState.IsKeyDown(Keys.Z))
            {
                if (theKeyboard.IsKeyDown(Keys.Z))
                    FireMissile(new Vector3(-550, -400, 0));
            }

            if (!LastKeyState.IsKeyDown(Keys.X))
            {
                if (theKeyboard.IsKeyDown(Keys.X))
                    FireMissile(new Vector3(0, -400, 0));
            }

            if (!LastKeyState.IsKeyDown(Keys.C))
            {
                if (theKeyboard.IsKeyDown(Keys.C))
                    FireMissile(new Vector3(550, -400, 0));
            }

            LastMouseState = theMouse;
            LastKeyState = theKeyboard;
        }

        void FireMissile(Vector3 basePos)
        {
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
                TheMissiles.Add(new TargetedMissile(Game, GameScale));
            }

            TheMissiles[freeOne].Spawn(basePos, Position);
        }

        void SetExplode(Vector3 position)
        {
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
                TheExplosions.Add(new Explosion(Game, GameScale));
            }

            TheExplosions[freeOne].Spawn(position);
        }

        void MoveUp()
        {
            if (Position.Y < Services.WindowHeight * 0.5f)
            {
                Velocity.Y = MoveSpeed;
                Velocity.X = 0;
            }
        }

        void MoveDown()
        {
            if (Position.Y > -Services.WindowHeight * 0.5f)
            {
                Velocity.Y = -MoveSpeed;
                Velocity.X = 0;
            }
        }

        void MoveRight()
        {
            if (Position.X < Services.WindowWidth * 0.5f)
            {
                Velocity.X = MoveSpeed;
                Velocity.Y = 0;
            }
        }

        void MoveLeft()
        {
            if (Position.X > -Services.WindowWidth * 0.5f)
            {
                Velocity.X = -MoveSpeed;
                Velocity.Y = 0;
            }
        }

        void StopMove()
        {
            Velocity = Vector3.Zero;
        }
    }
}

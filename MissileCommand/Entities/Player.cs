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
        List<Missile> Missiles;
        List<Mod> Xmarks;
        XnaModel XmarkModel;

        MouseState LastMouseState;
        KeyboardState LastKeyState;

        float MoveSpeed = 100;

        public Player(Game game, float gameScale) : base(game)
        {
            GameScale = gameScale;
            Xmarks = new List<Mod>();
            Missiles = new List<Missile>();

            LoadContent();
            BeginRun();
        }

        public override void Initialize()
        {
            DefuseColor = new Vector3(0.2f, 0.1f, 2.5f); // Reddish Blue

            base.Initialize();
        }

        public override void LoadContent()
        {
            LoadModel("MC_CrossHair");
            XmarkModel = Load("MC_Xmark");
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            GetInput();
            //MissileCollusion();

            base.Update(gameTime);
        }

        void MissileCollusion()
        {
            foreach (Missile missile in Missiles)
            {
                if (missile.Active)
                {
                    foreach (Mod xmark in Xmarks)
                    {
                        if (xmark.Active)
                        {
                            if (missile.SphereIntersect2D(xmark))
                            {
                                missile.Deactivate();
                                xmark.Active = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Screen resolution is 1200 X 900. Y positive on top of window. So up is positive.
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

            if (!LastKeyState.IsKeyDown(Keys.LeftControl))
            {
                if (theKeyboard.IsKeyDown(Keys.LeftControl))
                    FireMissile(new Vector3(0, -400, 0));
            }

            LastMouseState = theMouse;
            LastKeyState = theKeyboard;
        }

        void FireMissile(Vector3 basePos)
        {
            bool spawnNewMark = true;
            int freeMark = Xmarks.Count;

            for (int i = 0; i < Xmarks.Count; i++)
            {
                if (!Xmarks[i].Active)
                {
                    spawnNewMark = false;
                    freeMark = i;
                    break;
                }
            }

            if (spawnNewMark)
            {
                Xmarks.Add(new Mod(Game));
                Xmarks.Last().SetModel(XmarkModel);
                Xmarks.Last().Moveable = false;
            }

            Xmarks[freeMark].Position = Position;
            Xmarks[freeMark].DefuseColor = new Vector3(0, 0.1f, 2);
            Xmarks[freeMark].Active = true;

            bool spawnNewMissile = true;
            int freeMissile = Missiles.Count;

            for (int i = 0; i < Missiles.Count; i ++)
            {
                if (!Missiles[i].Active)
                {
                    spawnNewMissile = false;
                    freeMissile = i;
                    break;
                }
            }

            if (spawnNewMissile)
            {
                Missiles.Add(new Missile(Game, GameScale));
            }

            Missiles[freeMissile].Spawn(basePos, Xmarks[freeMark], 300);
            Missiles[freeMissile].TrailColor = new Vector3(0.1f, 0, 2);
            Missiles[freeMissile].TimerAmount = 0.06f;
        }

        void MoveUp()
        {
            if (Position.Y < Services.WindowHeight * 0.5f)
            {
                Velocity.Y = MoveSpeed;
            }
        }

        void MoveDown()
        {
            if (Position.Y > -Services.WindowHeight * 0.5f)
            {
                Velocity.Y = -MoveSpeed;
            }
        }

        void MoveRight()
        {
            if (Position.X < Services.WindowWidth * 0.5f)
            {
                Velocity.X = MoveSpeed;
            }
        }

        void MoveLeft()
        {
            if (Position.X > -Services.WindowWidth * 0.5f)
            {
                Velocity.X = -MoveSpeed;
            }
        }

        void StopMove()
        {
            Velocity = Vector3.Zero;
        }
    }
}

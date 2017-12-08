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

            base.Update(gameTime);
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
                    FireMissile();
            }

            LastMouseState = theMouse;
            LastKeyState = theKeyboard;
        }

        void FireMissile()
        {
            Xmarks.Add(new Mod(Game));
            Xmarks.Last().SetModel(XmarkModel);
            Xmarks.Last().Position = Position;
            Xmarks.Last().DefuseColor = new Vector3(0, 0.1f, 2);

            Missiles.Add(new Missile(Game, GameScale));
            Missiles.Last().Spawn(new Vector3(0, -400, 0), Position, 300);
            Missiles.Last().TrailColor = new Vector3(0.1f, 0, 2);
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

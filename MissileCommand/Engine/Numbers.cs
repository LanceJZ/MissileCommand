using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using Engine;

namespace Engine
{
    public class Numbers : GameComponent, IUpdateableComponent, ILoadContent, IBeginable
    {
        XnaModel[] NumberModels = new XnaModel[10];
        List<Engine.AModel> NumberEs = new List<Engine.AModel>();
        public Vector3 Position = Vector3.Zero;
        float Scale;

        public Numbers(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            Services.AddLoadable(this);

            base.Initialize();
        }

        public void LoadContent()
        {
            for (int i = 0; i < 10; i++)
            {
                NumberModels[i] = Game.Content.Load<XnaModel>("Models/Core/" + i.ToString());
            }
        }

        public void BeginRun()
        {

        }

        public void ProcessNumber(int number, Vector3 locationStart, float scale)
        {
            Position = locationStart;
            Scale = scale;

            UpdateNumber(number);
        }

        public void UpdateNumber(int number)
        {
            int numberIn = number;

            ClearNumbers();

            do
            {
                //Make digit the modulus of 10 from number.
                int digit = numberIn % 10;
                //This sends a digit to the draw function with the location and size.
                Engine.AModel numberE = InitiateNumber(digit);
                numberE.Scale = Scale;
                // Dividing the int by 10, we discard the digit that was derived from the modulus operation.
                numberIn /= 10;
                // Move the location for the next digit location to the left. We start on the right hand side
                // with the lowest digit.
            } while (numberIn > 0);

            UpdatePosition();
        }

        public void UpdatePosition()
        {
            float space = 0;

            foreach(Engine.AModel number in NumberEs)
            {
                number.Position = Position - new Vector3(space, 0, 0);
                number.MatrixUpdate();
                space += Scale * 11;
            }
        }

        public void ShowNumbers(bool show)
        {
            if (NumberEs != null)
            {
                foreach (Engine.AModel number in NumberEs)
                {
                    number.Active = show;
                }
            }
        }

        Engine.AModel InitiateNumber(int number)
        {
            Engine.AModel digit = new Engine.AModel(Game);

            if (number < 0)
                number = 0;

            digit.SetModel(NumberModels[number]);
            digit.Moveable = false;
            digit.ModelScale = new Vector3(Scale);

            NumberEs.Add(digit);

            return NumberEs.Last();
        }

        void RemoveNumber(Engine.AModel numberE)
        {
            NumberEs.Remove(numberE);
        }

        void ClearNumbers()
        {
            foreach(Engine.AModel digit in NumberEs)
            {
                digit.Destroy();
            }

            NumberEs.Clear();
        }
    }
}

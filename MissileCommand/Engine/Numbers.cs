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
        XnaModel[] NumberXNAModels = new XnaModel[10];
        List<AModel> NumberModels = new List<AModel>();
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
                NumberXNAModels[i] = Game.Content.Load<XnaModel>("Models/Core/" + i.ToString());
            }
        }

        public void BeginRun()
        {

        }

        public void ProcessNumber(int number, Vector3 locationStart, float scale)
        {
            Position = locationStart;
            Scale = scale;

            ChangeNumber(number);
        }

        public void ChangeNumber(int number, Vector3 defuseColor)
        {
            ChangeNumber(number);
            ChangeColor(defuseColor);
        }

        public void ChangeNumber(int number)
        {
            int numberIn = number;

            ClearNumbers();

            do
            {
                //Make digit the modulus of 10 from number.
                int digit = numberIn % 10;
                //This sends a digit to the draw function with the location and size.
                AModel numberE = InitiateNumber(digit);
                numberE.Scale = Scale;
                // Dividing the int by 10, we discard the digit that was derived from the modulus operation.
                numberIn /= 10;
                // Move the location for the next digit location to the left. We start on the right hand side
                // with the lowest digit.
            } while (numberIn > 0);

            ChangePosition();
        }

        public void ChangePosition()
        {
            float space = 0;

            foreach(AModel number in NumberModels)
            {
                number.Position = Position - new Vector3(space, 0, 0);
                number.MatrixUpdate();
                space += Scale * 11;
            }
        }

        public void ChangePosition(Vector3 position)
        {
            Position = position;
            ChangePosition();
        }

        public void ChangeColor(Vector3 defuseColor)
        {
            foreach (AModel number in NumberModels)
            {
                number.DefuseColor = defuseColor;
            }
        }

        public void ShowNumbers(bool show)
        {
            if (NumberModels != null)
            {
                foreach (AModel number in NumberModels)
                {
                    number.Active = show;
                }
            }
        }

        AModel InitiateNumber(int number)
        {
            AModel digit = new AModel(Game);

            if (number < 0)
                number = 0;

            digit.SetModel(NumberXNAModels[number]);
            digit.Moveable = false;
            digit.ModelScale = new Vector3(Scale);

            NumberModels.Add(digit);

            return NumberModels.Last();
        }

        void RemoveNumber(AModel numberE)
        {
            NumberModels.Remove(numberE);
        }

        void ClearNumbers()
        {
            foreach(AModel digit in NumberModels)
            {
                digit.Destroy();
            }

            NumberModels.Clear();
        }
    }
}

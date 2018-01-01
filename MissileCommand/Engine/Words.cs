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
    public class Words : GameComponent, IUpdateableComponent, ILoadContent, IBeginable
    {
        XnaModel[] WordXNAModels = new XnaModel[27];
        List<AModel> WordModels = new List<AModel>();
        float Scale;
        int TextSize;
        public Vector3 Position = Vector3.Zero;

        public Words (Game game) : base(game)
        {
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
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)(i + 65);

                WordXNAModels[i] = Game.Content.Load<XnaModel>("Models/Core/" + letter.ToString());
            }

            WordXNAModels[26] = Game.Content.Load<XnaModel>("Models/Core/UnderLine");
        }

        public void BeginRun()
        {

        }

        public void ProcessWords(string words, Vector3 locationStart, float scale)
        {
            Position = locationStart;
            Scale = scale;

            ChangeWords(words);
        }

        public void ChangeWords(string words, Vector3 defuseColor)
        {
            ChangeWords(words);
            ChangeColor(defuseColor);
        }

        public void ChangeWords(string words)
        {
            TextSize = words.Length;
            DeleteWords();

            foreach (char letter in words)
            {
                if ((int)letter > 64 && (int)letter < 91 || (int)letter == 95)
                {
                    int letval = (int)letter - 65;

                    if ((int)letter == 95)
                        letval = 26;

                    if (letval > -1 && letval < 27)
                    {
                        AModel letterE = InitiateLetter(letval);
                        letterE.Scale = Scale;
                    }

                }

                if ((int)letter == 32)
                {
                    WordModels.Add(new AModel(Game));
                }
            }

            ChangePosition();
        }

        public void ChangePosition()
        {
            float space = 0;

            foreach (AModel word in WordModels)
            {
                word.Position = Position - new Vector3(space, 0, 0);
                word.MatrixUpdate();
                space -= Scale * 11.5f;
            }
        }

        public void ChangePosition(Vector3 position)
        {
            Position = position;
            ChangePosition();
        }

        public void ChangeColor(Vector3 defuseColor)
        {
            foreach (AModel word in WordModels)
            {
                word.DefuseColor = defuseColor;
            }
        }

        AModel InitiateLetter(int letter)
        {
            AModel letterModel = new AModel(Game);
            letterModel.SetModel(WordXNAModels[letter]);
            letterModel.Moveable = false;
            letterModel.ModelScale = new Vector3(Scale);

            WordModels.Add(letterModel);

            return WordModels.Last();
        }

        public void DeleteWords()
        {
            foreach (AModel word in WordModels)
            {
                word.Destroy();
            }

            WordModels.Clear();
        }

        public void ShowWords(bool show)
        {
            if (WordModels != null)
            {
                foreach (AModel word in WordModels)
                {
                    word.Active = show;
                    word.Visable = show;
                }
            }
        }
    }
}

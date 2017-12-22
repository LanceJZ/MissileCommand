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
        XnaModel[] WordModels = new XnaModel[27];
        List<Engine.AModel> WordEs = new List<Engine.AModel>();
        float Scale;
        int TextSize;
        public Vector3 Position = Vector3.Zero;

        public Words (Game game) : base(game)
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
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)(i + 65);

                WordModels[i] = Game.Content.Load<XnaModel>("Models/Core/" + letter.ToString());
            }

            WordModels[26] = Game.Content.Load<XnaModel>("Models/Core/UnderLine");
        }

        public void BeginRun()
        {

        }

        public void ProcessWords(string words, Vector3 locationStart, float scale)
        {
            Position = locationStart;
            Scale = scale;

            UpdateWords(words);
        }

        public void UpdateWords(string words)
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
                        Engine.AModel letterE = InitiateLetter(letval);
                        letterE.Scale = Scale;
                    }

                }

                if ((int)letter == 32)
                {
                    WordEs.Add(new Engine.AModel(Game));
                }
            }

            UpdatePosition();
        }

        public void UpdatePosition()
        {
            float space = 0;

            foreach (Engine.AModel letter in WordEs)
            {
                letter.Position = Position - new Vector3(space, 0, 0);
                letter.MatrixUpdate();
                space -= Scale * 11.5f;
            }
        }

        Engine.AModel InitiateLetter(int letter)
        {
            Engine.AModel letterModel = new Engine.AModel(Game);
            letterModel.SetModel(WordModels[letter]);
            letterModel.Moveable = false;
            letterModel.ModelScale = new Vector3(Scale);

            WordEs.Add(letterModel);

            return WordEs.Last();
        }

        public void DeleteWords()
        {
            foreach (Engine.AModel word in WordEs)
            {
                word.Destroy();
            }

            WordEs.Clear();
        }

        public void ShowWords(bool show)
        {
            if (WordEs != null)
            {
                foreach (Engine.AModel word in WordEs)
                {
                    word.Active = show;
                    word.Visable = show;
                }
            }
        }
    }
}

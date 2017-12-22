using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;

namespace MissileCommand.Entities
{
    struct HighScoreListModels
    {
        Numbers TheRank;
        Numbers TheScore;
        Words TheName;

        public Numbers Rank
        {
            get => TheRank;
            set => TheRank = value;
        }

        public Numbers Score
        {
            get
            {
                return TheScore;
            }

            set
            {
                TheScore = value;
            }
        }

        public Words Name
        {
            get
            {
                return TheName;
            }

            set
            {
                TheName = value;
            }
        }
    }

    struct HighScoreList
    {
        int TheScore;
        string TheName;

        public int Score
        {
            get
            {
                return TheScore;
            }

            set
            {
                TheScore = value;
            }
        }

        public string Name
        {
            get
            {
                return TheName;
            }

            set
            {
                TheName = value;
            }
        }
    }

    public class HighScoresUI : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        GameLogic GameLogicRef;


        public HighScoresUI(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;

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

            base.Update(gameTime);
        }
    }
}

﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;
using Engine;

namespace MissileCommand.Entities
{
    public class UI : GameComponent, IBeginable, IUpdateableComponent, ILoadContent
    {
        GameLogic GameLogicRef;
        float GameScale;

        Numbers TheScoreDisplay;
        Words TheScoreText;
        Words TheGameOverText;
        Words TheStartANewGameText;

        KeyboardState OldKeyState;

        public Numbers ScoreDisplay { get => TheScoreDisplay; }

        public UI(Game game, GameLogic gameLogic, float gameScale) : base(game)
        {
            GameScale = gameScale;
            GameLogicRef = gameLogic;

            TheScoreDisplay = new Numbers(game);
            TheScoreText = new Words(game);
            TheGameOverText = new Words(game);
            TheStartANewGameText = new Words(game);

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
            TheScoreDisplay.ProcessNumber(0, new Vector3(-250, 400, 100), 2);
            TheScoreText.ProcessWords("SCORE", new Vector3(-550, 400, 100), 2);
            TheGameOverText.ProcessWords("GAME OVER", new Vector3(-160, 100, 100), 4);
            TheStartANewGameText.ProcessWords("PRESS ENTER TO START GAME", new Vector3(-120, 0, 100), 1);
        }

        public override void Update(GameTime gameTime)
        {

            SwitchState();
            OldKeyState = Keyboard.GetState();

            base.Update(gameTime);
        }

        void SwitchState()
        {
            switch (GameLogicRef.CurrentMode)
            {
                case GameState.Attract:
                    Attract();
                    break;
                case GameState.InPlay:
                    GamePlay();
                    break;
                case GameState.Over:
                    GameOver();
                    break;
                case GameState.HighScore:
                    HighScore();
                    break;
            }
        }

        void Attract()
        {
            KeyboardState KBS = Keyboard.GetState();

            GameLogicRef.GameOver();
        }

        void GameOver()
        {
            KeyboardState KBS = Keyboard.GetState();

            if (!OldKeyState.IsKeyDown(Keys.Enter) && KBS.IsKeyDown(Keys.Enter))
            {
                GameLogicRef.NewGame();
            }

            TheGameOverText.ShowWords(true);
            TheStartANewGameText.ShowWords(true);
        }

        void HighScore()
        {
            KeyboardState KBS = Keyboard.GetState();

        }

        void GamePlay()
        {
            KeyboardState KBS = Keyboard.GetState();

            TheGameOverText.ShowWords(false);
            TheStartANewGameText.ShowWords(false);
        }
    }
}

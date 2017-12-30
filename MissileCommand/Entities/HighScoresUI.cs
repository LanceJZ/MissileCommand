using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        Words[] Instructions = new Words[4];
        HighScoreList[] HighScoreData = new HighScoreList[10];
        HighScoreListModels[] HighScoreModels = new HighScoreListModels[10];
        Words NewHighScoreWords;
        Words HighScoreWords;
        Words HighScoresWords;
        Words BonusCityWords;
        Numbers HighScoreNumbers;
        Numbers BonusCityPointNumbers;

        KeyboardState OldKeyState;

        FileStream TheFileStream;

        char[] HighScoreSelectedLetters = new char[3];
        string[] Instructiontxt = new string[4];
        string Filename = "Score.sav";
        string DataRead = "";

        int NewHighScoreRank;
        int HighScore;
        int HighScoreSelector;

        public HighScoresUI(Game game, GameLogic gameLogic) : base(game)
        {
            GameLogicRef = gameLogic;

            for (int i = 0; i < 10; i++)
            {
                HighScoreModels[i].Name = new Words(game);
                HighScoreModels[i].Rank = new Numbers(game);
                HighScoreModels[i].Score = new Numbers(game);
            }

            for (int i=0; i < Instructions.Length; i++)
            {
                Instructions[i] = new Words(game);
            }

            NewHighScoreWords = new Words(game);
            HighScoreWords = new Words(game);
            BonusCityWords = new Words(game);
            HighScoresWords = new Words(game);
            HighScoreNumbers = new Numbers(game);
            BonusCityPointNumbers = new Numbers(game);

            // Screen resolution is 1200 X 900.
            // Y positive on top of window. So down is negative.
            // X positive is right of window. So to the left is negative.
            // Z positive is towards the front. So to place objects behind other objects, put them in the negative.
            game.Components.Add(this);
        }

        public override void Initialize()
        {
            NewHighScoreRank = 0;
            HighScore = 0;
            HighScoreSelector = 0;

            base.Initialize();
            Services.AddLoadable(this);
            Services.AddBeginable(this);
        }

        public void LoadContent()
        {

        }

        public void BeginRun()
        {
            Instructiontxt[0] = "GREAT SCORE";
            Instructiontxt[1] = "ENTER YOUR INITIALS";
            Instructiontxt[2] = "LEFT OR RIGHT TO CHANGE LETTERS";
            Instructiontxt[3] = "PRESS LEFT CTRL TO SELECT";

            for (int i = 0; i < 4; i++)
            {
                Instructions[i].ProcessWords(Instructiontxt[i],
                    new Vector3(-Instructiontxt[i].Length * 10, 180 - (i * 30), 10), 2);
            }

            //Instructions[0].ProcessWords(, new Vector3(-220, 180, 10), 4);
            //Instructions[1].ProcessWords(, new Vector3(-190, 60, 10), 2);
            //Instructions[2].ProcessWords(, new Vector3(-300, 30, 10), 2);
            //Instructions[3].ProcessWords(, new Vector3(-240, 0, 10), 2);
            BonusCityWords.ProcessWords("BONUS CITY EVERY       POINTS",
                new Vector3(-320, -350, 10), 2);
            BonusCityPointNumbers.ProcessNumber(GameLogicRef.BonusCityAmount,
                new Vector3(150, -350, 10), 2);
            HighScoresWords.ProcessWords("HIGH SCORES", new Vector3(-110, 150, 10), 2);
            HighScoreWords.ProcessWords("HIGH SCORE", new Vector3(-100, 400, 10), 2);
            HighScoreNumbers.ProcessNumber(0, new Vector3(300, 400, 10), 2);
            NewHighScoreWords.ProcessWords("", new Vector3(-30, -120, 10), 2);
            NewHighScoreWords.ShowWords(false);
            ShowInstructions(false);

            Vector3 Position = new Vector3(-20, 100, 10);

            int p = 0;

            foreach (HighScoreListModels list in HighScoreModels)
            {
                list.Rank.ProcessNumber(p + 1, new Vector3(Position.X - 150, Position.Y - p * 30, 10), 2);
                list.Name.ProcessWords("ZIM", new Vector3(Position.X, Position.Y - p * 30, 10), 2);
                list.Score.ProcessNumber(0, new Vector3(Position.X + 250, Position.Y - p * 30, 10), 2);
                p++;
            }

            if (ReadFile())
            {
                DataDecode();
                ProcessHighScoreList();
            }
            else
            {
                int score = 7500;

                for (int i = 0; i < 10; i++)
                {
                    score += (25 * Services.RandomMinMax(10, 20));

                    HighScoreData[i].Name = "ZIM";
                    HighScoreData[i].Score = score;

                    score -= (25 * Services.RandomMinMax(40, 45));
                }

                p = 0;

                foreach (HighScoreListModels list in HighScoreModels)
                {
                    list.Score.UpdateNumber(HighScoreData[p].Score);
                    p++;
                }

                HighScore = HighScoreData[0].Score;
                HighScoreNumbers.UpdateNumber(HighScore);
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (GameLogicRef.CurrentMode)
            {
                case GameState.HighScore:
                    NewHighScore();
                    OldKeyState = Keyboard.GetState();
                    break;
            }

            base.Update(gameTime);
        }

        public void NewGame()
        {

            ShowHighScoreList(false);
        }

        public void ShowInstructions(bool show)
        {
            foreach (Words line in Instructions)
            {
                line.ShowWords(show);
            }
        }

        public void ShowHighScoreList(bool show)
        {
            foreach(HighScoreListModels score in HighScoreModels)
            {
                score.Name.ShowWords(show);
                score.Rank.ShowNumbers(show);
                score.Score.ShowNumbers(show);
            }

            BonusCityPointNumbers.ShowNumbers(show);
            BonusCityWords.ShowWords(show);
            HighScoresWords.ShowWords(show);
        }

        public bool CheckHighScore(int score)
        {
            for (int rank = 0; rank < 10; rank++)
            {
                if (score > HighScoreData[rank].Score)
                {
                    if (rank < 9)
                    {
                        HighScoreList[] oldScores = new HighScoreList[10];

                        for (int oldranks = rank; oldranks < 10; oldranks++)
                        {
                            oldScores[oldranks].Score = HighScoreData[oldranks].Score;
                            oldScores[oldranks].Name = HighScoreData[oldranks].Name;
                        }

                        for (int newranks = rank; newranks < 9; newranks++)
                        {
                            HighScoreData[newranks + 1].Score = oldScores[newranks].Score;
                            HighScoreData[newranks + 1].Name = oldScores[newranks].Name;
                        }
                    }

                    HighScoreData[rank].Score = score;
                    HighScoreData[rank].Name = "AAA";
                    WriteFile();
                    HighScoreSelector = 0;
                    NewHighScoreRank = rank;
                    ShowInstructions(true);
                    HighScoreSelectedLetters = "___".ToCharArray();
                    NewHighScoreWords.UpdateWords("___");
                    NewHighScoreWords.ShowWords(true);
                    return true;
                }
            }

            return false;
        }

        void NewHighScore()
        {
            KeyboardState keyState = Keyboard.GetState();

            if (!OldKeyState.IsKeyDown(Keys.LeftControl) && keyState.IsKeyDown(Keys.LeftControl))
            {
                HighScoreSelector++;

                ProcessLettersSelected();

                if (HighScoreSelector > 2)
                {
                    ProcessNewHighScore();
                }

                return;
            }

            if (!OldKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyDown(Keys.Right))
            {
                HighScoreSelectedLetters[HighScoreSelector]++;

                if (HighScoreSelectedLetters[HighScoreSelector] > 95)
                    HighScoreSelectedLetters[HighScoreSelector] = (char)65;

                if (HighScoreSelectedLetters[HighScoreSelector] > 90)
                    HighScoreSelectedLetters[HighScoreSelector] = (char)95;

                ProcessLettersSelected();
                return;
            }

            if (!OldKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyDown(Keys.Left))
            {
                HighScoreSelectedLetters[HighScoreSelector]--;

                if (HighScoreSelectedLetters[HighScoreSelector] == 94)
                    HighScoreSelectedLetters[HighScoreSelector] = (char)90;

                if (HighScoreSelectedLetters[HighScoreSelector] < 65)
                    HighScoreSelectedLetters[HighScoreSelector] = (char)95;

                ProcessLettersSelected();
            }
        }

        void ProcessLettersSelected()
        {
            NewHighScoreWords.UpdateWords(ProcessLettersToString());
        }

        void ProcessNewHighScore()
        {
            GameLogicRef.SwitchToAttract();
            HighScoreData[NewHighScoreRank].Name = ProcessLettersToString();
            ProcessHighScoreList();
            ShowInstructions(false);
            WriteFile();
            NewHighScoreWords.ShowWords(false);
            GameLogicRef.SwitchToAttract();
        }

        string ProcessLettersToString()
        {
            string name = "";

            for (int i = 0; i < 3; i++)
            {
                name += HighScoreSelectedLetters[i].ToString();
            }

            return name;
        }

        void ProcessHighScoreList()
        {
            for (int i = 0; i < 10; i++)
            {
                if (HighScoreData[i].Score > 0)
                {
                    HighScoreModels[i].Score.UpdateNumber(HighScoreData[i].Score);
                    HighScoreModels[i].Name.UpdateWords(HighScoreData[i].Name);

                    if (HighScoreData[i].Score > HighScore)
                        HighScore = HighScoreData[i].Score;
                }
            }

            HighScoreNumbers.UpdateNumber(HighScore);
        }

        void DataDecode()
        {
            int scoreRank = 0;
            int letter = 0;
            bool isLetter = true;
            string fromNumber = "";

            foreach (char DataChar in DataRead)
            {
                if (DataChar.ToString() == "*")
                    break;

                if (DataChar.ToString() == "'\0'")
                    break;

                if (isLetter)
                {
                    letter++;
                    HighScoreData[scoreRank].Name += DataChar;

                    if (letter == 3)
                        isLetter = false;
                }
                else
                {
                    if (DataChar.ToString() == ":")
                    {
                        HighScoreData[scoreRank].Score = int.Parse(fromNumber);

                        scoreRank++;

                        if (scoreRank > 9)
                            break;

                        letter = 0;
                        fromNumber = "";
                        isLetter = true;
                    }
                    else
                    {
                        fromNumber += DataChar.ToString();
                    }
                }
            }
        }

        bool ReadFile()
        {
            if (File.Exists(Filename))
            {
                TheFileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);

                byte[] dataByte = new byte[1024];
                UTF8Encoding bufferUTF8 = new UTF8Encoding(true);

                while (TheFileStream.Read(dataByte, 0, dataByte.Length) > 0)
                {
                    DataRead += bufferUTF8.GetString(dataByte, 0, dataByte.Length);
                }

                Close();
            }
            else
                return false;

            return true;
        }

        void WriteFile()
        {
            TheFileStream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write);

            for (int i = 0; i < 10; i++)
            {
                if (HighScoreData[i].Score > 0)
                {
                    byte[] name = new UTF8Encoding(true).GetBytes(HighScoreData[i].Name);
                    TheFileStream.Write(name, 0, name.Length);

                    byte[] score = new UTF8Encoding(true).GetBytes(HighScoreData[i].Score.ToString());
                    TheFileStream.Write(score, 0, score.Length);

                    byte[] marker = new UTF8Encoding(true).GetBytes(":");
                    TheFileStream.Write(marker, 0, marker.Length);
                }
            }

            Close();
        }

        void Close()
        {
            TheFileStream.Flush();
            TheFileStream.Close();
            TheFileStream.Dispose();
        }
    }
}

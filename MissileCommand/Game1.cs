using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Engine;

namespace MissileCommand
{
    using GameServices = Services;
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager Graphics;
        SpriteBatch spriteBatch;
        GameLogic TheGame;

        KeyboardState OldKeyState;
        bool PauseGame = false;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.IsFullScreen = false;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.PreferredBackBufferWidth = 1200;
            Graphics.PreferredBackBufferHeight = 900;
            Graphics.PreferMultiSampling = true; //Error in MonoGame 3.6 for DirectX, fixed in next dev version.
            Graphics.PreparingDeviceSettings += SetMultiSampling;
            Graphics.ApplyChanges();
            Graphics.GraphicsDevice.RasterizerState = new RasterizerState(); //Must be after Apply Changes.
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";

            TheGame = new GameLogic(this);
        }

        private void SetMultiSampling(object sender, PreparingDeviceSettingsEventArgs eventArgs)
        {
            PresentationParameters PresentParm = eventArgs.GraphicsDeviceInformation.PresentationParameters;
            PresentParm.MultiSampleCount = 8;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Positive Y is Up. Positive X is Right.
            GameServices.Initialize(Graphics, this, new Vector3(0, 0, 500), 0, 1000);
            // Setup lighting.
            GameServices.DefuseLight = new Vector3(0.6f, 0.5f, 0.7f);
            GameServices.LightDirection = new Vector3(-0.75f, -0.75f, -0.5f);
            GameServices.SpecularColor = new Vector3(0.1f, 0, 0.5f);
            GameServices.AmbientLightColor = new Vector3(0.25f, 0.25f, 0.25f); // Add some overall ambient light.

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void BeginRun()
        {
            GameServices.BeginRun(); //This only happens once in a game.

            base.BeginRun();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState KBS = Keyboard.GetState();

            if (TheGame.CurrentMode == GameState.InPlay)
            {
                if (!OldKeyState.IsKeyDown(Keys.P) && KBS.IsKeyDown(Keys.P))
                    PauseGame = !PauseGame;
            }

            OldKeyState = Keyboard.GetState();

            if (!PauseGame)
                base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.05f, 0, 0.1f));

            base.Draw(gameTime);
        }
    }
}

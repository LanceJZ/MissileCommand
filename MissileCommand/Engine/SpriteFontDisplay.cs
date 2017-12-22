using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Engine
{
    public class SpriteFontDisplay : SpritePositionedObject, IDrawComponent
    {
        SpriteFont m_SpriteFont;
        Color m_TintColor = Color.White;
        string m_String;

        public string String
        {
            set { m_String = value; }
        }

        public Color TintColor
        {
            set { m_TintColor = value; }
        }

        public SpriteFontDisplay(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            Services.AddDrawableComponent(this);
        }

        public virtual void LoadContent()
        {

        }

        public void Initialize(SpriteFont spriteFont, Vector2 position)
        {
            m_SpriteFont = spriteFont;
            Position = position;
        }

        public override void BeginRun()
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Draw()
        {
            if (Active)
            {
                Services.SpriteBatch.DrawString(m_SpriteFont, m_String, Position, m_TintColor);
            }
        }
    }
}

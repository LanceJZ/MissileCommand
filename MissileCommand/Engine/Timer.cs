using Microsoft.Xna.Framework;

namespace Engine
{
    public class Timer : GameComponent
    {
        private float m_Seconds = 0;
        private float m_Amount = 0;

        public float Seconds
        {
            get { return m_Seconds; }
        }

        public float Amount
        {
            get
            {
                return m_Amount;
            }

            set
            {
                m_Amount = value;
                Reset();
            }
        }

        public bool Expired
        {
            get
            {
                return m_Seconds > m_Amount;
            }
        }

        public Timer(Game game) : base(game)
        {
            Game.Components.Add(this);
        }

        public Timer (Game game, float amount) : base(game)
        {
            Amount = amount;
            Game.Components.Add(this);
        }

        public override void Initialize()
        {

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Expired)
                m_Seconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Reset()
        {
            Enabled = true;
            m_Seconds = 0;
        }

        public void Reset(float time)
        {
            m_Amount = time;
            Reset();
        }
    }
}

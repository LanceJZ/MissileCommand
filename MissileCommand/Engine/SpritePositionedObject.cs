#region Using
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#endregion Using

namespace Engine
{
    public class SpritePositionedObject : GameComponent, IBeginable
    {
        #region Fields

        private float m_ElapsedGameTime;

        // Doing these as fields is almost twice as fast as if they were properties.
        // Also, sense XYZ are fields they do not get data binned as a property.
        public List<SpritePositionedObject> Children;
        public Vector2 Position;
        public Vector2 Acceleration;
        public Vector2 Velocity;
        public Vector2 ReletivePosition;
        public Rectangle AABB; // The axis-aligned bounding box.
        private BasicEffect renderer; // The basic effect used to render the AABB.
        private Matrix m_ViewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1f), Vector3.Zero, Vector3.Up);
        private Matrix m_WorldMatrix = Matrix.CreateTranslation(0, 0, 0); // The GameModel's transformation matrix.
        private Matrix m_ProjectionMatrix;
        private short[] indexData; // The index array used to render the AABB.
        private VertexPositionColor[] aabbVertices; // The AABB vertex array (used for rendering).
        private float m_RotationInRadians;
        private float m_ReletiveRotation;
        private float m_ScalePercent = 1;
        private float m_RotationVelocity;
        private float m_RotationAcceleration;
        private float m_Radius;
        private bool m_Hit = false;
        private bool m_ExplosionActive = false;
        private bool m_Pause = false;
        private bool m_Movable = true;
        private bool m_Active = true;
        private bool m_ActiveDependent;
        private bool m_DirectConnection;
        private bool m_Parent;
        private bool m_Child; //Used in Sprite class only.
        private bool m_Debug;

        #endregion Fields

        #region Properties

        public float ElapsedGameTime { get { return m_ElapsedGameTime; } }

        public float RotationInRadians
        {
            get { return m_RotationInRadians; }

            set { m_RotationInRadians = value; }
        }

        public float ReletiveRotation
        {
            get { return m_ReletiveRotation; }

            set { m_ReletiveRotation = value; }
        }

        public float Scale
        {
            get { return m_ScalePercent; }

            set { m_ScalePercent = value; }
        }

        public float RotationVelocity
        {
            get { return m_RotationVelocity; }

            set { m_RotationVelocity = value; }
        }

        public float RotationAcceleration
        {
            get { return m_RotationAcceleration; }

            set { m_RotationAcceleration = value; }
        }

        public float Radius
        {
            get { return m_Radius; }

            set { m_Radius = value; }
        }

        public bool Parent
        {
            set { m_Parent = value; }
            get { return m_Parent; }
        }

        public bool Child
        {
            get { return m_Child; }
            set { m_Child = value; }
        }

        public bool Hit
        {
            get { return m_Hit; }

            set { m_Hit = value; }
        }

        public bool ExplosionActive
        {
            get { return m_ExplosionActive; }

            set { m_ExplosionActive = value; }
        }

        public bool Pause
        {
            get { return m_Pause; }

            set { m_Pause = value; }
        }

        public bool Movable
        {
            get { return m_Movable; }

            set
            { m_Movable = value; }
        }

        public bool Active
        {
            get { return m_Active; }

            set { m_Active = value; }
        }

        public bool ActiveDependent
        {
            get { return m_ActiveDependent; }

            set { m_ActiveDependent = value; }
        }

        public bool DirectConnection
        {
            get { return m_DirectConnection; }

            set { m_DirectConnection = value; }
        }

        /// <summary>036
        /// Gets or sets the GameModel's AABB.037
        /// </summary>
        public bool Debug
        {
            set { m_Debug = value; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// This is the constructor that gets the Positioned Object ready for use and adds it to the Drawable Components list.
        /// </summary>
        /// <param name="game">The game class</param>
        public SpritePositionedObject(Game game) : base(game)
        {
            Children = new List<SpritePositionedObject>();
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Allows the game component to be updated.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Movable && Active)
            {
                base.Update(gameTime);

                m_ElapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Velocity += Acceleration * m_ElapsedGameTime;
                Position += Velocity * m_ElapsedGameTime;
                AABB.X = (int)(Position.X);
                AABB.Y = (int)(Position.Y);
                RotationVelocity += RotationAcceleration * m_ElapsedGameTime;
                RotationInRadians += RotationVelocity * m_ElapsedGameTime;

                if (RotationInRadians > MathHelper.TwoPi)
                    RotationInRadians = 0;

                if (RotationInRadians < 0)
                    RotationInRadians = MathHelper.TwoPi;
            }

            if (m_Parent)
            {
                foreach (SpritePositionedObject child in Children)
                {
                    if (Active)
                    {
                        if (child.DirectConnection)
                        {
                            child.Position = Position;
                            child.RotationInRadians = RotationInRadians;
                            child.Scale = Scale;
                        }
                        else
                        {
                            child.Position = Vector2.Transform(child.ReletivePosition,
                                Matrix.CreateRotationZ(RotationInRadians));
                            child.Position += Position;
                            child.RotationInRadians = RotationInRadians + child.ReletiveRotation;
                        }
                    }

                    if (child.ActiveDependent)
                        child.Active = Active;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            AABB = new Rectangle();
            Services.AddBeginable(this);
        }

        public virtual void BeginRun()
        {
            m_ProjectionMatrix = Matrix.CreateOrthographic(Services.WindowWidth, Services.WindowHeight, 1, 2);
        }

        public virtual void AddChild(SpritePositionedObject child, bool activeDependent, bool directConnection)
        {
            Children.Add(child);
            Children[Children.Count - 1].ActiveDependent = activeDependent;
            Children[Children.Count - 1].DirectConnection = directConnection;
            //Children[Children.Count - 1].Movable = true;

            m_Parent = true;
        }

        public void SetAABB(Vector2 heightWidth)
        {
            AABB = new Rectangle((int)Position.X, (int)Position.Y, (int)(heightWidth.X * m_ScalePercent),
                (int)(heightWidth.Y * m_ScalePercent));
        }

        public void RemoveComp()
        {
            Game.Components.Remove(this);
        }

        public void AddComp()
        {
            Game.Components.Add(this);
        }

        public bool CirclesIntersect(Vector2 Target, float TargetRadius)
        {
            float distanceX = Target.X - Position.X;
            float distanceY = Target.Y - Position.Y;
            float radius = Radius + TargetRadius;

            if ((distanceX * distanceX) + (distanceY * distanceY) < radius * radius)
                return true;

            return false;
        }

        #endregion Public Methods

        #region Draw AABB

        private void SetupRenderer()
        {
            // Create a new BasicEffect instance.
            renderer = new BasicEffect(Game.GraphicsDevice);

            // This lets you color the AABB.
            renderer.VertexColorEnabled = true;

            // Set renderer matrices.
            renderer.World = m_WorldMatrix;
            renderer.View = m_ViewMatrix;
            renderer.Projection = m_ProjectionMatrix;
        }

        private void DebugDraw(BoundingBox aabb, Color color)
        {
            // Setup the debug renderer.
            SetupRenderer();

            // Create an array to store the AABB's vertices.
            aabbVertices = new VertexPositionColor[8];

            // Get an array of points that make up the corners of the AABB.
            Vector3[] corners = aabb.GetCorners();

            // Fill the AABB vertex array.
            for (int i = 0; i < 8; i++)
            {
                aabbVertices[i].Position = corners[i];
                aabbVertices[i].Color = color;
            }

            // Create the index array
            indexData = new short[] { 0, 1, 1, 2, 2, 3, 3, 0, 0, 4, 1, 5, 2, 6, 3, 7, 4, 5, 5, 6, 6, 7, 7, 4, };

            // Loop through each effect pass.
            foreach (EffectPass pass in renderer.CurrentTechnique.Passes)
            {
                // Draw AABB.
                Game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                    aabbVertices, 0, 8, indexData, 0, 12);
            }
        }

        #endregion Draw AABB
    }
}
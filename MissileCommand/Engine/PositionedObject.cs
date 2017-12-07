#region Using
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Engine
{
	public class PositionedObject : GameComponent, IBeginable
	{
		#region Fields
		private float m_ElapsedGameTime;
		// Doing these as fields is almost twice as fast as if they were properties.
		// Also, sense XYZ are fields they do not get data binned as a property.
		public PositionedObject ParentPO;
		public Vector3 Position = Vector3.Zero;
		public Vector3 Acceleration = Vector3.Zero;
		public Vector3 Velocity = Vector3.Zero;
		public Vector3 ParentPosition = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 ParentRotation = Vector3.Zero;
        public Vector3 WorldPosition = Vector3.Zero;
        public Vector3 WorldRotation = Vector3.Zero;
        public Vector3 RotationVelocity = Vector3.Zero;
        public Vector3 RotationAcceleration = Vector3.Zero;
		//short[] indexData; // The index array used to render the AABB.
		//VertexPositionColor[] aabbVertices; // The AABB vertex array (used for rendering).
		float m_ScalePercent = 1;
		float m_Radius = 0;
        Vector2 m_HeightWidth;
		bool m_Hit = false;
		bool m_ExplosionActive = false;
		bool m_Pause = false;
		bool m_Moveable = true;
		bool m_Active = true;
		bool m_ActiveDependent;
		bool m_DirectConnection;
		bool m_Parent;
		bool m_Child;
		bool m_Debug;
		#endregion
		#region Properties
		public float ElapsedGameTime { get => m_ElapsedGameTime; }
        /// <summary>
        /// Scale by percent of original. If base of sprite, used to enlarge sprite.
        /// </summary>
		public float Scale	{ get => m_ScalePercent; set => m_ScalePercent = value; }
        /// <summary>
        /// Used for circle collusion. Sets radius of circle.
        /// </summary>
		public float Radius { get => m_Radius; set => m_Radius = value; }
        /// <summary>
        /// Enabled means this class is a parent, and has at least one child.
        /// </summary>
		public bool Parent { get => m_Parent; set => m_Parent = value; }
        /// <summary>
        /// Enabled means this class is a child to a parent.
        /// </summary>
		public bool Child { get => m_Child; set => m_Child = value; }
        /// <summary>
        /// Enabled tells class hit by another class.
        /// </summary>
		public bool Hit { get => m_Hit; set => m_Hit = value; }
        /// <summary>
        /// Enabled tells class an explosion is active.
        /// </summary>
		public bool ExplosionActive { get => m_ExplosionActive; set => m_ExplosionActive = value; }
        /// <summary>
        /// Enabled pauses class update.
        /// </summary>
		public bool Pause { get => m_Pause; set => m_Pause = value; }
        /// <summary>
        /// Enabled will move using velocity and acceleration.
        /// </summary>
		public bool Moveable { get => m_Moveable; set => m_Moveable = value; }
        /// <summary>
        /// Enabled causes the class to update. If base of Sprite, enables sprite to be drawn.
        /// </summary>
		public bool Active { get => m_Active; set => m_Active = value; }
        /// <summary>
        /// Enabled the active bool will mirror that of the parent.
        /// </summary>
		public bool ActiveDependent { get => m_ActiveDependent; set => m_ActiveDependent = value; }
        /// <summary>
        /// Enabled the position and rotation will always be the same as the parent.
        /// </summary>
		public bool DirectConnection { get => m_DirectConnection; set => m_DirectConnection = value; }
		/// <summary>
		/// Gets or sets the GameModel's AABB
		/// </summary>
		public bool Debug { set => m_Debug = value; }

        public Vector2 WidthHeight { get => m_HeightWidth; set => m_HeightWidth = value; }

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, (int)WidthHeight.X, (int)WidthHeight.Y);
            }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// This is the constructor that gets the Positioned Object ready for use and adds it to the Drawable Components list.
        /// </summary>
        /// <param name="game">The game class</param>
        public PositionedObject(Game game) : base(game)
		{
			game.Components.Add(this);
		}
        #endregion
        #region Public Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        public virtual void BeginRun()
        {

        }
        /// <summary>
        /// Allows the game component to be updated.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
		{
			if (Moveable && Active)
			{
				base.Update(gameTime);

				m_ElapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
				Velocity += Acceleration * m_ElapsedGameTime;
				Position += Velocity * m_ElapsedGameTime;
				RotationVelocity += RotationAcceleration * m_ElapsedGameTime;
				Rotation += RotationVelocity * m_ElapsedGameTime;
            }

            if (m_Child)
			{
                if (DirectConnection)
                {
                    Position = ParentPO.Position;
                    Rotation = ParentPO.Rotation;
                }
                else
                {
                    ParentPosition = ParentPO.Position;
                    ParentRotation = ParentPO.Rotation;
                    WorldPosition = Position + ParentPosition + ParentPO.ParentPosition;
                    WorldRotation = Rotation + ParentRotation + ParentPO.ParentRotation;
                }

                if (ActiveDependent)
					Active = ParentPO.Active;

                base.Update(gameTime);
            }
        }
        /// <summary>
        /// Add PO class or base PO class from AModel or Sprite as child of this class.
        /// </summary>
        /// <param name="Parent">The parent to this class.</param>
        /// <param name="activeDependent">Bind Active to child.</param>
        /// <param name="directConnection">Bind Position and Rotation to child.</param>
		public virtual void AddAsChildOf(PositionedObject Parent, bool activeDependent, bool directConnection)
		{
            ActiveDependent = activeDependent;
            DirectConnection = directConnection;
            Child = true;
            ParentPO = Parent;
            ParentPO.Parent = true;
		}

        public void Remove()
		{
			Game.Components.Remove(this);
		}
        /// <summary>
        /// Circle collusion detection. Target circle will be compared to this class's.
        /// Will return true of they intersect.
        /// </summary>
        /// <param name="Target">Position of target.</param>
        /// <param name="TargetRadius">Radius of target.</param>
        /// <returns></returns>
		public bool CirclesIntersect(Vector3 Target, float TargetRadius)
		{
			float distanceX = Target.X - Position.X;
			float distanceY = Target.Y - Position.Y;
			float radius = Radius + TargetRadius;

			if ((distanceX * distanceX) + (distanceY * distanceY) < radius * radius)
				return true;

			return false;
		}
        /// <summary>
        /// Circle collusion detection. Target circle will be compared to this class's.
        /// Will return true of they intersect.
        /// </summary>
        /// <param name="Target">Target Positioned Object.</param>
        /// <returns></returns>
		public bool CirclesIntersect(PositionedObject Target)
        {
            float distanceX = Target.Position.X - Position.X;
            float distanceY = Target.Position.Y - Position.Y;
            float radius = Radius + Target.Radius;

            if ((distanceX * distanceX) + (distanceY * distanceY) < radius * radius)
                return true;

            return false;
        }
        /// <summary>
        /// Returns a Vector3 direction of travel from angle and magnitude.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="magnitude"></param>
        /// <returns>Vector2</returns>
        public Vector3 SetVelocity(float angle, float magnitude)
        {
            Vector3 vel = Vector3.Zero;
            vel.Y = (float)(Math.Sin(angle) * magnitude);
            vel.X = (float)(Math.Cos(angle) * magnitude);
            return vel;
        }
        /// <summary>
        /// Returns a float of the angle in radians derived from two Vector2 passed into it, using only the X and Y.
        /// </summary>
        /// <param name="origin">Vector2 of origin</param>
        /// <param name="target">Vector2 of target</param>
        /// <returns>Float</returns>
        public float AngleFromVectors(Vector3 origin, Vector3 target)
        {
            return (float)(Math.Atan2(target.Y - origin.Y, target.X - origin.X));
        }

        public float RandomRadian()
        {
            return Services.RandomMinMax(0, MathHelper.TwoPi);
        }

        public Vector3 SetRandomVelocity(float speed)
        {
            float ang = RandomRadian();
            float amt = Services.RandomMinMax(speed * 0.15f, speed);
            return SetVelocityFromAngle(ang, amt);
        }

        public Vector3 SetRandomVelocity(float speed, float radianDirection)
        {
            float amt = Services.RandomMinMax(speed * 0.15f, speed);
            return SetVelocityFromAngle(radianDirection, amt);
        }

        public Vector3 SetVelocityFromAngle(float rotation, float magnitude)
        {
            return new Vector3((float)Math.Cos(rotation) * magnitude, (float)Math.Sin(rotation) * magnitude, 0);
        }

        public Vector3 SetVelocity3FromAngleZ(float rotationZ, float magnitude)
        {
            return new Vector3((float)Math.Cos(rotationZ) * magnitude, (float)Math.Sin(rotationZ) * magnitude, 0);
        }

        public Vector2 SetVelocityFromAngle(float magnitude)
        {
            float ang = RandomRadian();
            return new Vector2((float)Math.Cos(ang) * magnitude, (float)Math.Sin(ang) * magnitude);
        }

        public Vector2 SetRandomEdge()
        {
            return new Vector2(Services.WindowWidth * 0.5f,
                Services.RandomMinMax(-Services.WindowHeight * 0.45f, Services.WindowHeight * 0.45f));
        }

        public float AimAtTarget(Vector3 target, float facingAngle, float magnitude)
        {
            float turnVelocity = 0;
            float targetAngle = AngleFromVectors(Position, target);
            float targetLessFacing = targetAngle - facingAngle;
            float facingLessTarget = facingAngle - targetAngle;

            if (Math.Abs(targetLessFacing) > MathHelper.Pi)
            {
                if (facingAngle > targetAngle)
                {
                    facingLessTarget = ((MathHelper.TwoPi - facingAngle) + targetAngle) * -1;
                }
                else
                {
                    facingLessTarget = (MathHelper.TwoPi - targetAngle) + facingAngle;
                }
            }

            if (facingLessTarget > 0)
            {
                turnVelocity = -magnitude;
            }
            else
            {
                turnVelocity = magnitude;
            }

            return turnVelocity;
        }

        public void CheckWindowBorders()
        {
            if (Position.X > Services.WindowWidth)
                Position.X = 0;

            if (Position.X < 0)
                Position.X = Services.WindowWidth;

            if (Position.Y > Services.WindowHeight)
                Position.Y = 0;

            if (Position.Y < 0)
                Position.Y = Services.WindowHeight;
        }

        public void CheckWindowSideBorders(int width)
        {
            if (Position.X + width > Services.WindowWidth)
                Position.X = 0;

            if (Position.X < 0)
                Position.X = Services.WindowWidth - width;
        }

        #endregion
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using System.Collections.Generic;
using System;

namespace Engine
{
    public class SModel : PositionedObject, IDrawComponent
    {

        XnaModel m_Model;
        Matrix[] Transforms = null;
        Matrix LocalMatrix;


        public XnaModel LoadedModel { get => m_Model; set => m_Model = value; }

        public SModel(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            Services.AddDrawableComponent(this);

        }

        public override void BeginRun()
        {
            base.BeginRun();

            if (LoadedModel == null)
                return;

            Transforms = new Matrix[LoadedModel.Bones.Count];
            LoadedModel.CopyAbsoluteBoneTransformsTo(Transforms);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {
                // Calculate the mesh transformation by combining translation, rotation, and scaling
                LocalMatrix = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z)
                    * Matrix.CreateTranslation(Position);
            }
        }

        public void Draw(GameTime gametime)
        {
            if (Active)
            {
                if (LoadedModel == null)
                    return;

                foreach (ModelMesh mesh in LoadedModel.Meshes)
                {
                    Matrix localWorld = Transforms[mesh.ParentBone.Index] * LocalMatrix;

                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.LightingEnabled = true; // Turn on the lighting subsystem.
                        effect.PreferPerPixelLighting = true;
                        effect.DirectionalLight0.DiffuseColor = Services.DefuseLight;
                        effect.DirectionalLight0.Direction = Services.LightDirection;
                        effect.DirectionalLight0.SpecularColor = Services.SpecularColor;
                        effect.AmbientLightColor = Services.AmbientLightColor;

                        effect.World = localWorld;

                        Services.Camera.Draw(effect);
                    }

                    mesh.Draw();
                }
            }

        }
    }
}

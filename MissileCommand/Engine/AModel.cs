#region Using
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
#endregion

namespace Engine
{
    public class AModel : PositionedObject, IDrawComponent, ILoadContent
    {
        public XnaModel XNAModel { get; private set; }
        public bool Visable { get => m_Visable; set => m_Visable = value; }
        public Vector3 DefuseColor = new Vector3(1,1,1);
        Texture2D XNATexture;
        private Matrix[] ModelTransforms;
        private Matrix BaseWorld;
        bool m_Visable = true;

        public AModel (Game game) : base(game)
        {

        }

        public AModel(Game game, XnaModel model) : base(game)
        {
            SetModel(model);
        }

        public AModel(Game game, XnaModel model, Texture2D texture) : base(game)
        {
            SetModel(model, texture);
        }

        public override void Initialize()
        {
            Enabled = true;
            Services.AddDrawableComponent(this);
            Services.AddLoadable(this);

            Services.GraphicsDM.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Services.GraphicsDM.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            Services.GraphicsDM.GraphicsDevice.BlendState = BlendState.Opaque;
            Services.GraphicsDM.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.Initialize();
        }

        public override void BeginRun()
        {

            base.BeginRun();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            /* A rule of thumb is ISROT - Identity, Scale, Rotate, Orbit, Translate.
               This is the order to multiple your matrices in.
               So for the moon and earth example, to place the moon:
                Identity - this is just Matrix.Identity (an all 1's matrix).
                Scale - Scale the moon to it's proper size.
                Rotate - rotate the moon around it's own center
                Orbit - this is a two step Translate then Rotate process,
                        first Translate (move) the moon to it's position relative to the
                        earth (i.e. if the earth was at 0, 0, 0).  The rotate the moon around
                        this point to position it in orbit.
                Translate - move the moon to the final location, which will be the same
                        as the location of earth in this case since it's already setup to be in orbit.*/
            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            BaseWorld = Matrix.Identity;

            BaseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            if (Child)
            {
                    BaseWorld *= Matrix.CreateFromYawPitchRoll(ParentPO.Rotation.Y + ParentPO.ParentRotation.Y,
                        ParentPO.Rotation.X + ParentPO.ParentRotation.X,
                        ParentPO.Rotation.Z + ParentPO.ParentRotation.Z)
                        * Matrix.CreateTranslation(ParentPO.Position + ParentPO.ParentPosition);
            }
        }

        public void Draw(GameTime gametime)
        {
            if (Active && Visable)
            {
                if (XNAModel == null)
                    return;

                foreach (ModelMesh mesh in XNAModel.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        BasicEffect effect = (BasicEffect)meshPart.Effect;

                        //effect.Texture = XNATexture ?? effect.Texture; //Replace texture if XNATexture is not null.
                        effect.EnableDefaultLighting();
                        if (DefuseColor != Vector3.Zero)
                            effect.DiffuseColor = DefuseColor;
                        effect.PreferPerPixelLighting = true;
                        effect.World = BaseWorld;
                        if (XNATexture != null)
                            effect.Texture = XNATexture;// ?? effect.Texture; //Replace texture if XNATexture is not null.
                        Services.Camera.Draw(effect);
                    }

                    mesh.Draw();
                }
            }
        }

        public void SetModel(XnaModel model)
        {
            if (model != null)
            {
                XNAModel = model;

                ModelTransforms = new Matrix[XNAModel.Bones.Count];
                XNAModel.CopyAbsoluteBoneTransformsTo(ModelTransforms);
            }
        }

        public void SetModel(XnaModel model, Texture2D texture)
        {
            XNATexture = texture;

            SetModel(model);
        }

        public void LoadModel(string modelName)
        {
            if (modelName != null)
                XNAModel = Game.Content.Load<XnaModel>("Models/" + modelName);

            SetModel(XNAModel);
        }

        public void LoadModel(string modelName, string textureName)
        {
            if (textureName != null)
                XNATexture = Game.Content.Load<Texture2D>("Textures/" + textureName);

            LoadModel(modelName);
        }

        public XnaModel Load(string modelName)
        {
            return Game.Content.Load<XnaModel>("Models/" + modelName);
        }

        public SoundEffect LoadSoundEffect(string soundName)
        {
            return Game.Content.Load<SoundEffect>("Sounds/" + soundName);
        }

        public virtual void LoadContent()
        {
            //This method intentionally left blank.
        }

        public void Destroy()
        {
            if (ModelTransforms != null)
                ModelTransforms.Initialize();

            BaseWorld = Matrix.Identity;
            XNAModel = null;
            XNATexture = null;

            Dispose();
        }
    }
}

#region Using
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Engine
{
    public class AnimatedModel : PositionedObject
    {
        #region Fields
        private Matrix worldMatrix;
        private Matrix worldTranslate;
        private Matrix worldRotate;
        private Matrix worldScale;
        private Matrix baseWorld;
        Matrix[] modelBoneTransforms;
        private BoundingSphere proximitySphere;

        public BoundingPartList _modelBoundingParts;

        #endregion

        #region Properties
        public XnaModel xnaModelMesh
        {
            get;
            set;
        }

        public BoundingSphere ProximitySphere
        {
            get
            {
                return proximitySphere;
            }
        }
        #endregion

        #region Constructor
        public AnimatedModel(Game game)
            : base(game)
        {
            //game.Components.Add(this);
        }
        #endregion

        #region Public Methods
        public void Load(string modelFileName, bool hasMeshInfoFile)
        {
            xnaModelMesh = Game.Content.Load<XnaModel>(modelFileName);

            InitializeProximitySphere(1);

            if (hasMeshInfoFile)
            {
                LoadMeshInfo(xnaModelMesh, modelFileName);
                _modelBoundingParts = ExtractBoundingParts();
            }
            else
            {
                CreateMeshInfo(xnaModelMesh);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            //UpdateMatrixes(); //Was needed to stop the flash, now it no longer flashes without it.
            Scale = 1;
            proximitySphere = new BoundingSphere();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //UpdateMatrixes();

            // Calculate the base transformation by combining
            // translation, rotation, and scaling
            baseWorld = Matrix.CreateScale(Scale)
               * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
               * Matrix.CreateTranslation(Position);

            UpdateProximitySphere();
        }

        public void Draw()
        {

            foreach (ModelMesh mesh in xnaModelMesh.Meshes)
            {
                //Matrix localWorld = modelBoneTransforms[mesh.ParentBone.Index] * worldMatrix;

                Matrix localWorld = modelBoneTransforms[mesh.ParentBone.Index] * baseWorld;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BasicEffect effect = (BasicEffect)meshPart.Effect;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = localWorld;
                    Services.Camera.Draw(effect);
                }

                mesh.Draw();
            }

        }

        public void UpdateModel()
        {

        }


        //public override void Draw(GameTime gameTime)
        //{
        //    base.Draw(gameTime);

        //    for (int index = 0; index < ModelMesh.Meshes.Count; index++)
        //    {
        //        Matrix localWorld = ModelMesh.Meshes[index].ParentBone.Transform * worldMatrix;

        //        foreach (BasicEffect effect in ModelMesh.Meshes[index].Effects)
        //        {
        //            effect.EnableDefaultLighting();
        //            effect.PreferPerPixelLighting = true;
        //            effect.World = localWorld;

        //            Services.Camera.Draw(effect);
        //        }

        //        ModelMesh.Meshes[index].Draw();
        //    }
        //}

        public BoundingPartList ExtractBoundingParts()
        {
            BoundingPartList instance = _modelBoundingParts.Clone();
            return instance;
        }

        public bool DetectCollision(BoundingSphere boundingSphere)
        {
            return _modelBoundingParts.Intersects(boundingSphere);
        }

        public bool DetectCollision(BoundingPart boundingPart)
        {
            return _modelBoundingParts.Intersects(boundingPart);
        }

        public void LoadMeshInfo(XnaModel model, string modelFileName)
        {
            _modelBoundingParts = new BoundingPartList();

            //Matrix[] modelBoneTransforms = new Matrix[model.Bones.Count];
            modelBoneTransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(modelBoneTransforms);

            XmlDocument meshInfo = new XmlDocument();

            if (File.Exists(string.Format(@".\MeshInfo\{0}.xml", modelFileName)))
            {
                meshInfo.Load(string.Format(@".\MeshInfo\{0}.xml", modelFileName));
            }

            int meshIndex = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                string xpath = string.Format("/MeshInfo/MeshPart[@id='{0}']", mesh.Name);
                XmlNode meshInfoPart = meshInfo.SelectSingleNode(xpath);
                XmlNodeList meshInfoBoundingParts = null;
                if (meshInfoPart != null)
                {
                    xpath = "BoundingParts/BoundingPart";
                    meshInfoBoundingParts = meshInfoPart.SelectNodes(xpath);
                }

                if ((meshInfoBoundingParts != null) && (meshInfoBoundingParts.Count > 0))
                {
                    BoundingSphere[] pieces = new BoundingSphere[meshInfoBoundingParts.Count];

                    int pieceIndex = 0;

                    Color boundingPartColour = new Color(byte.Parse(meshInfoPart.Attributes["color.bounds.r"].Value),
                                                            byte.Parse(meshInfoPart.Attributes["color.bounds.g"].Value),
                                                            byte.Parse(meshInfoPart.Attributes["color.bounds.b"].Value));

                    foreach (XmlNode subdivisionNode in meshInfoBoundingParts)
                    {
                        float x0 = float.Parse(subdivisionNode.Attributes["x"].Value);
                        float y0 = float.Parse(subdivisionNode.Attributes["y"].Value);
                        float z0 = float.Parse(subdivisionNode.Attributes["z"].Value);
                        float w0 = float.Parse(subdivisionNode.Attributes["w"].Value);
                        Vector4 subdivision = new Vector4(x0, y0, z0, w0);

                        //Determine the new BoundingSphere's Radius
                        float radius = subdivision.W * mesh.BoundingSphere.Radius;

                        //Determine the new BoundingSphere's Center by interpolating.
                        //The subdivision's X, Y, Z represent percentages in each axis.
                        //They will used across the full diameter of XNA's "default" BoundingSphere.
                        float x =
                            MathHelper.Lerp(mesh.BoundingSphere.Center.X - mesh.BoundingSphere.Radius,
                                            mesh.BoundingSphere.Center.X + mesh.BoundingSphere.Radius, subdivision.X);
                        float y =
                            MathHelper.Lerp(mesh.BoundingSphere.Center.Y - mesh.BoundingSphere.Radius,
                                            mesh.BoundingSphere.Center.Y + mesh.BoundingSphere.Radius, subdivision.Y);
                        float z =
                            MathHelper.Lerp(mesh.BoundingSphere.Center.Z - mesh.BoundingSphere.Radius,
                                            mesh.BoundingSphere.Center.Z + mesh.BoundingSphere.Radius, subdivision.Z);
                        Vector3 center = new Vector3(x, y, z);

                        pieces[pieceIndex] = new BoundingSphere(center, radius);

                        pieceIndex++;
                    }

                    BoundingPart boundingPart =
                        new BoundingPart(mesh.BoundingSphere, pieces, modelBoneTransforms[mesh.ParentBone.Index],
                                         mesh.Name, boundingPartColour);

                    _modelBoundingParts.Add(boundingPart);
                }
                meshIndex++;
            }
        }

        public void CreateMeshInfo(XnaModel model)
        {
            _modelBoundingParts = new BoundingPartList();

            //Matrix[] modelBoneTransforms = new Matrix[model.Bones.Count];
            modelBoneTransforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(modelBoneTransforms);
            BoundingSphere[] pieces = new BoundingSphere[1];

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingPart boundingPart =
                    new BoundingPart(mesh.BoundingSphere, pieces, modelBoneTransforms[mesh.ParentBone.Index],
                                     mesh.Name, Color.White);

                _modelBoundingParts.Add(boundingPart);
            }
        }

        #endregion

        protected void UpdateMatrixes()
        {
            worldTranslate = Matrix.CreateTranslation(Position);
            worldScale = Matrix.CreateScale(Scale);
            worldRotate = Matrix.CreateRotationX(Rotation.X);
            worldRotate *= Matrix.CreateRotationY(Rotation.Y);
            worldRotate *= Matrix.CreateRotationZ(Rotation.Z);
            worldMatrix = worldScale * worldRotate * worldTranslate;
        }

        protected void InitializeProximitySphere(float scale)
        {
            proximitySphere.Radius = 0;

            foreach (ModelMesh mesh in xnaModelMesh.Meshes)
            {
                proximitySphere.Radius += (mesh.BoundingSphere.Radius * scale);
            }
        }

        private void UpdateProximitySphere()
        {
            proximitySphere.Center = Position;
        }
    }
}

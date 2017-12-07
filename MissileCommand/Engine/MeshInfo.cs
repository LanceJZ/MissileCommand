using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace Engine
{
    public class MeshInfo
    {
        // This class is thanks to Sharky 3D Collision Detection tutor at http://sharky.bluecog.co.nz/?page_id=113

        private BoundingPartList _modelBoundingParts;
        private float _scaleModifier = 1f;

        public float ScaleModifier
        {
            get { return _scaleModifier; }
        }

        public BoundingPartList ExtractBoundingParts()
        {
            BoundingPartList instance = _modelBoundingParts.Clone();
            return instance;
        }

        public MeshInfo(XnaModel model, string contentName)
        {
            Load(model, contentName);
        }

        private void Load(XnaModel model, string contentName)
        {
            _modelBoundingParts = new BoundingPartList();

            Matrix[] modelBoneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelBoneTransforms);

            XmlDocument meshInfo = new XmlDocument();

            if (File.Exists(string.Format(@".\MeshInfo\{0}.xml", contentName)))
            {
                meshInfo.Load(string.Format(@".\MeshInfo\{0}.xml", contentName));
            }

            if ((!meshInfo.DocumentElement.HasAttributes) || (meshInfo.DocumentElement.Attributes["scaleModifier"] == null))
            {
                _scaleModifier = 1f;
            }
            else
            {
                _scaleModifier = float.Parse(meshInfo.DocumentElement.Attributes["scaleModifier"].Value);
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

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class BoundingPart
    {
        // This class is thanks to Sharky 3D Collision Detection tutor at http://sharky.bluecog.co.nz/?page_id=113

        private bool _spheresTransformed = false;

        private string _name;
        private Color _color;
        private List<BoundingSphere> _boundingSpheres;
        private List<BoundingSphere> _boundingSpheresTransformed;
        private BoundingSphere _proximitySphere;
        private BoundingSphere _proximitySphereTransformed;
        private Matrix _boneTransform;
        private Matrix _world = Matrix.Identity;

        public bool SpheresTransformed
        {
            get
            {
                return _spheresTransformed;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }

        public List<BoundingSphere> BoundingSpheres
        {
            get
            {
                return _boundingSpheres;
            }
            set
            {
                _boundingSpheres = value;
            }
        }

        public List<BoundingSphere> BoundingSpheresTransformed
        {
            get
            {
                return _boundingSpheresTransformed;
            }
            set
            {
                _boundingSpheresTransformed = value;
            }
        }

        public BoundingSphere ProximitySphere
        {
            get
            {
                return _proximitySphere;
            }
            set
            {
                _proximitySphere = value;
            }
        }

        public BoundingSphere ProximitySphereTransformed
        {
            get
            {
                return _proximitySphereTransformed;
            }
            set
            {
                _proximitySphereTransformed = value;
            }
        }

        private Matrix BoneTransform
        {
            get
            {
                return _boneTransform;
            }
            set
            {
                _boneTransform = value;
            }
        }

        public BoundingPart(BoundingSphere proximitySphere, BoundingSphere[] boundingSpheres, Matrix boneTransform,
            string name, Color color)
        {
            _name = name;
            _color = color;
            _boneTransform = boneTransform;

            _proximitySphere = new BoundingSphere(proximitySphere.Center, proximitySphere.Radius);
            _proximitySphereTransformed = new BoundingSphere(proximitySphere.Center, proximitySphere.Radius);

            _boundingSpheres = new List<BoundingSphere>();
            _boundingSpheresTransformed = new List<BoundingSphere>();

            _boundingSpheres.InsertRange(0, boundingSpheres);

            _boundingSpheresTransformed.InsertRange(0, boundingSpheres);
        }

        public BoundingPart Clone()
        {
            BoundingPart clone = new BoundingPart(_proximitySphere, _boundingSpheres.ToArray(), _boneTransform, _name, _color);
            return clone;
        }

        /// <summary>
        /// Returns True if any of this BoundingPart's BoundingSpheres collide with the given BoundingPart's.
        /// </summary>
        /// <param name="boundingPart"></param>
        /// <returns></returns>
        public bool Intersects(BoundingPart boundingPart)
        {
            //For optimization we will only bother checking for collisions if the proximitySphere shows a collision.
            if (this.ProximitySphereTransformed.Intersects(boundingPart.ProximitySphereTransformed))
            {
                //the actual collision spheres may not have been transformed yet.
                if (!_spheresTransformed)
                {
                    TransformSpheres();
                }

                if (!boundingPart.SpheresTransformed)
                {
                    boundingPart.TransformSpheres();
                }


                foreach (BoundingSphere thisBS in _boundingSpheresTransformed)
                {
                    foreach (BoundingSphere otherBS in boundingPart.BoundingSpheresTransformed)
                    {
                        if (thisBS.Intersects(otherBS))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns True if any of this BoundingPart's BoundingSpheres collide with the given BoundingSphere.
        /// </summary>
        /// <param name="boundingPart"></param>
        /// <returns></returns>
        public bool Intersects(BoundingSphere boundingSphere)
        {
            if (this.ProximitySphereTransformed.Intersects(boundingSphere))
            {
                //For optimization we may have not transformed the actual parts yet.
                //We will only bother if the Region shows a collision.
                if (!_spheresTransformed)
                {
                    TransformSpheres();
                }

                foreach (BoundingSphere thisBS in _boundingSpheresTransformed)
                {
                    if (thisBS.Intersects(boundingSphere))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        private void TransformProximitySphere()
        {
            Matrix localWorld = _world;
            _proximitySphereTransformed.Center = Vector3.Transform(_proximitySphere.Center, localWorld);

            //BoneTransforms can include some scaling, so scaling the radius by our defined scale may not be sufficient.
            //Big thanks to Shawn Hargreaves (http://blogs.msdn.com/shawnhar/default.aspx) for the following solution
            //using localWorld.Forward.Length() instead.
            _proximitySphereTransformed.Radius = _proximitySphere.Radius * localWorld.Forward.Length();

            _spheresTransformed = false;
        }

        public void TransformSpheres()
        {
            int bsIndex = 0;
            foreach (BoundingSphere bs in _boundingSpheres)
            {
                Matrix localWorld = _world;
                BoundingSphere bsTransformed = _boundingSpheresTransformed[bsIndex];
                bsTransformed.Center = Vector3.Transform(bs.Center, localWorld);

                //BoneTransforms can include some scaling, so scaling the radius by our defined scale may not be sufficient.
                //Big thanks to Shawn Hargreaves (http://blogs.msdn.com/shawnhar/default.aspx) for the following solution
                //using localWorld.Forward.Length() instead.
                bsTransformed.Radius = bs.Radius * localWorld.Forward.Length();

                _boundingSpheresTransformed[bsIndex] = bsTransformed;

                bsIndex++;
            }
            _spheresTransformed = true;
        }

        /// <summary>
        /// Depending on the value of optimize, perform a Transform each of the part's BoundingSpheres into it's World Space
        /// position, scale & orientation.
        /// The resulting transformed BoundingSphere's can then be used to test for Collisions.
        /// </summary>
        /// <param name="worldMatrix"></param>
        /// <param name="regionOnly"></param>
        public void Transform(Matrix world, bool optimize)
        {
            _world = world;

            TransformProximitySphere();
            if (!optimize)
            {
                TransformSpheres();
            }
        }

        /// <summary>
        /// Perform a non-optimized Transform each of the part's BoundingSpheres into it's World Space position,
        /// scale & orientation.
        /// The resulting transformed BoundingSphere's can then be used to test for Collisions.
        /// </summary>
        /// <param name="worldMatrix"></param>
        public void Transform(Matrix worldMatrix)
        {
            Transform(worldMatrix, false);
        }

    }
}

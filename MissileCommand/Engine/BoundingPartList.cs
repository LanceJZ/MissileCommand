using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class BoundingPartList : List<BoundingPart>
    {
        // This class is thanks to Sharky 3D Collision Detection tutor at http://sharky.bluecog.co.nz/?page_id=113

        public BoundingPartList()
            : base()
        {
        }

        public BoundingPartList Clone()
        {
            BoundingPartList clone = new BoundingPartList();
            foreach (BoundingPart part in this)
            {
                clone.Add(part.Clone());
            }

            return clone;
        }

        public bool Intersects(BoundingPart boundingPart)
        {
            bool result = false;

            foreach (BoundingPart part in this)
            {
                if (part.Intersects(boundingPart))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Intersects(BoundingSphere boundingSphere)
        {
            bool result = false;

            foreach (BoundingPart part in this)
            {
                if (part.Intersects(boundingSphere))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }


        public void Transform(Matrix world, bool optimize)
        {
            foreach (BoundingPart bp in this)
            {
                bp.Transform(world, optimize);
            }
        }

        public void Transform(Matrix world)
        {
            Transform(world, false);
        }

    }
}

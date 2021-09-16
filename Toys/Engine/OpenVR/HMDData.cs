using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.VR
{
    public class HMDData
    {
        public Quaternion Rotation;
        public Vector3 Position;
        public Quaternion StartRotation;
        public Vector3 StartPosition;
        public int Id;

        public Vector3 GetPosition
        {
            get { return Position - StartPosition; }
        }

        public Quaternion GetRotation
        {
            get { return Rotation.Inverted() * StartRotation; }
        }
    }


}

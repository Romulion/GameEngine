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
        public Matrix4[] Prejections = new Matrix4[2];

        public Vector3 GetPosition
        {
            get { return StartPosition - Position; }
        }

        public Quaternion GetRotation
        {
            get { return StartRotation * Rotation.Inverted(); }
        }
    }


}

using System;
using OpenTK;

namespace Toys
{

    public class Transform
    {
        private Matrix4 localT;
        private SceneNode baseNode;
        Quaternion rotationQuaternion;
		Vector3 rotation;
		Vector3 position;
        Vector3 scale;

        /// <summary>
        /// Get Matrix in World Coordinates
        /// </summary>
        public Matrix4 GlobalTransform
        {
            get; private set;
        } 

        internal Transform(SceneNode node)
        {
            localT = Matrix4.Identity;
            GlobalTransform = Matrix4.Identity;
            rotationQuaternion = Quaternion.Identity;
            rotation = Vector3.Zero;
            position = Vector3.Zero;
            baseNode = node;
            scale = Vector3.One;
        }
    
        /// <summary>
        /// Local Transformation Matrix (Model space)
        /// </summary>
        public Matrix4 GetTransform
        {
            get { return localT; }
        }

        /// <summary>
        /// Position of Node Relative to Parent
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                RecalculateLocal();
            }
        }

        /// <summary>
        /// Scale of Node Object
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
                RecalculateLocal();
            }
        }


        /// <summary>
        /// Euler Angle Rotation; Order XYZ
        /// </summary>
        public Vector3 Rotation
        {
            get
            {
                return rotation;
            }

            set
            {
                rotation = value;
                rotationQuaternion = Quaternion.FromEulerAngles(rotation);
                RecalculateLocal();
            }
        }

        /// <summary>
        /// Rotation Quaternion
        /// </summary>
        public Quaternion RotationQuaternion
        {
            get
            {
                return rotationQuaternion;
            }

            set
            {
                rotationQuaternion = value;
                rotation = rotationQuaternion.ToEulerXYZ();
                RecalculateLocal();
            }
        }

        internal void UpdateGlobalTransform()
        {
            if (baseNode.Parent != null)
                GlobalTransform = baseNode.Parent.GetTransform.GlobalTransform * localT;
            else
                GlobalTransform = localT;
        }

        private void RecalculateLocal()
        {
            localT = Matrix4.CreateScale(scale) * Matrix4.CreateFromQuaternion(rotationQuaternion) * Matrix4.CreateTranslation(position);
        }
    }
}

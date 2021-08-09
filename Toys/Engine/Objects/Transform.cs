using System;
using OpenTK.Mathematics;

namespace Toys
{

    public class Transform : Component
    {
        private Matrix4 localT;
        private SceneNode baseNode;
        Quaternion rotationQuaternion;
		Vector3 rotation;
		Vector3 position;
        Vector3 scale;
        const float ratio = (float)(Math.PI / 180);

        /// <summary>
        /// Get Matrix in World Coordinates
        /// </summary>
        public Matrix4 GlobalTransform
        {
            get; private set;
        } 

        internal Transform(SceneNode node) : base (typeof(Transform))
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
                Quaternion.ToEulerAngles(rotationQuaternion, out rotation);
                return rotation / ratio;
            }

            set
            {
                rotation = value * ratio;
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
                rotation = rotationQuaternion.ToEulerAngles();
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
            UpdateGlobalTransform();
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
        }

        internal override void RemoveComponent()
        {
            Node = null;
        }
        internal override void Unload()
        {
        }
    }
}

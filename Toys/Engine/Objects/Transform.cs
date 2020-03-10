using System;
using OpenTK;

namespace Toys
{

    public class Transform
    {
        private Matrix4 localT;
        private SceneNode baseNode;
        Quaternion rotationQuaternion;
		//Vector3 rotation = new Vector3();
		Vector3 position = new Vector3();

        // public Vector3 scale;
        public Matrix4 GlobalTransform
        {
            get; private set;
        } 

        public Transform(SceneNode node)
        {
            localT = Matrix4.Identity;
            GlobalTransform = Matrix4.Identity;
            rotationQuaternion = Quaternion.Identity;
            baseNode = node;
        }

        /// <summary>
        /// Make transform to object
        /// </summary>
        /// <param name="local">Local transformation</param>
        /// <param name="parent">Parent transformation</param>
        public void Transformation(Matrix4 local)
        {
            localT = localT * local;
            //baseNode.UpdateTransform();
        }

    
        public void SetTransform(Matrix4 local)
        {
            localT = local;
            //baseNode.UpdateTransform();
        }


        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                var rot = localT.ClearTranslation();
                position = value;
                localT = rot * Matrix4.CreateTranslation(value);
                baseNode.UpdateTransform();
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return localT.ExtractRotation().Xyz;
            }

            set
            {
                var trans = localT.ClearRotation();
                localT = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(value)) * trans;
            }
        }

        public Quaternion RotationQuaternion
        {
            get
            {
                return rotationQuaternion;
            }

            set
            {
                var trans = localT.ClearRotation();
                rotationQuaternion = value;
                localT = Matrix4.CreateFromQuaternion(value) * trans;
            }
        }

        public void UpdateGlobalTransform()
        {
            if (baseNode.Parent != null)
                GlobalTransform = baseNode.Parent.GetTransform.GlobalTransform * localT;
            else
                GlobalTransform = localT;
        }
    }
}

using System;
using OpenTK;

namespace Toys
{

    public class Transform
    {
        private Matrix4 localT;
        private SceneNode baseNode;
		Vector3 rotation = new Vector3();
		Vector3 position = new Vector3();

        // public Vector3 scale;
        public Matrix4 globalTransform
        {
            get; private set;
        } 

        public Transform(SceneNode node)
        {
            localT = Matrix4.Identity;
            globalTransform = Matrix4.Identity;
            baseNode = node;
        }

        /// <summary>
        /// Make transform to object
        /// </summary>
        /// <param name="local">Local transformation</param>
        /// <param name="parent">Parent transformation</param>
        public void Transformation(Matrix4 local, Matrix4 parent)
        {
            localT = localT * local;
            baseNode.UpdateTransform();
        }

    
        public void SetTransform(Matrix4 local, Matrix4 parent)
        {
            localT = local;
            baseNode.UpdateTransform();
        }


        public Vector3 Position
        {
            get
            {
                return localT.ExtractTranslation();
            }

            set
            {
                var rot = localT.ClearTranslation();
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
                var rot = localT.ClearRotation();
                localT = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(value)) * rot;
                baseNode.UpdateTransform();
            }
        }

        public void UpdateGlobalTransform()
        {
            if (baseNode.Parent != null)
                globalTransform = baseNode.Parent.GetTransform.globalTransform * localT;
            else
                globalTransform = localT;
        }
    }
}

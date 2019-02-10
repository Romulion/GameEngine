using System;
using OpenTK;

namespace Toys
{

    class Transformation
    {
        private Matrix4 localT;
        private SceneNode basenode;
        // public Vector3 scale;
        public Matrix4 globalTransform
        {
            get; private set;
        } 

        public Transformation(SceneNode node)
        {
            localT = Matrix4.Identity;
            globalTransform = Matrix4.Identity;
            basenode = node;
        }

        /// <summary>
        /// Make transform to object
        /// </summary>
        /// <param name="local">Local transformation</param>
        /// <param name="parent">Parent transformation</param>
        public void Transform(Matrix4 local, Matrix4 parent)
        {
            localT = localT * local;
            basenode.UpdateTransform();
        }

    
        public void SetTransform(Matrix4 local, Matrix4 parent)
        {
            localT = local;
            basenode.UpdateTransform();
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
                basenode.UpdateTransform();
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
                localT = rot * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(value));
                basenode.UpdateTransform();
            }
        }

        public void UpdateGlobalTransform()
        {
            if (basenode.parent != null)
                globalTransform = basenode.parent.GetTransform.globalTransform * localT;
            else
                globalTransform = localT;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys
{
    public class Node2BoneSyncScript : ScriptingComponent
    {
        public BoneTransform Bone;
        /// <summary>
        /// Node offset in bone space
        /// </summary>
        public Vector3 OffsetPosition = Vector3.Zero;
        /// <summary>
        /// Node rotaion offset in bone space
        /// </summary>
        public Quaternion OffsetRotation = Quaternion.Identity;

        void Update()
        {
            if (Bone != null) {
                var mat = Bone.TransformMatrix;
                mat = mat * Matrix4.CreateTranslation(OffsetPosition) * Matrix4.CreateFromQuaternion(OffsetRotation);
                Node.GetTransform.Position = mat.ExtractTranslation();
                Node.GetTransform.RotationQuaternion = mat.ExtractRotation();
            }
        }
    }
}

using OpenTK;

namespace Toys
{
    public class BoneTransform
    {
        public Bone Bone;
        Matrix4 TransformMatrix;
        Matrix4 LocalSpace;
        Matrix4 LocalSpaceInverted;
        public Matrix4 LocalMatrix;
        Matrix4 BoneMatrix;
        Vector3 InitialOffset;

        Vector3 Translation;
        Vector3 Rotation;
        Vector3 Scale;

        Vector3 LocalTranslation;
        Vector3 LocalRotation;
        Vector3 LocalScale;

        //IK Variables
        public Quaternion IKRotation;
        public Quaternion LocalRotationIKLink;
        Vector3 LocalTranslationIKLink;

        public bool IsIK;
        public bool IsIKLink;
        public bool IsAddLocal;
        public bool IsRotateAdd;
        public bool IsTranslateAdd;
        public bool PhysicsIKSkip;
        public bool LocalRotationFlag;

        public BoneTransform Parent;
        IKResolver IK;
        
        bool changed;

        public BoneTransform(Bone b)
        {
            Bone = b;
            LocalMatrix = Matrix4.Identity;
            LocalSpace = Matrix4.CreateTranslation(-Bone.Position);
            LocalSpaceInverted = Matrix4.CreateTranslation(Bone.Position);
            IsIK = Bone.IK;
            IKRotation = Quaternion.Identity;
            ResetTransform(false);
        }

        public void UpdateLocalMatrix(bool ik = true)
        {
            Quaternion rot = Quaternion.FromEulerAngles(Rotation);
            Vector3 trans = Translation;

           
           if (IsIKLink)
            {
                LocalRotationIKLink = rot;
                LocalTranslationIKLink = trans;
                rot *= IKRotation;
            }
           
            LocalMatrix = Matrix4.CreateFromQuaternion(rot);

            if (Scale.X != 1f || Scale.Y != 1f || Scale.Z != 1f)
                LocalMatrix *= Matrix4.CreateScale(Scale);

            LocalMatrix += Matrix4.CreateTranslation(trans);
            BoneMatrix = LocalMatrix;
            LocalMatrix += Matrix4.CreateTranslation(InitialOffset);

            if (Parent != null)
            {
                LocalScale = Vector3.Multiply(Parent.LocalScale, Scale);
                LocalMatrix *= Parent.LocalMatrix;
            }
            else
            {
                LocalScale = Scale;
            }
            if (ik && IsIK && IK != null && (!PhysicsIKSkip || !IK.IsPhysicsAllLink))
            {
                IK.Transform();
            }
        }

        public void UpdateLocalMatrixIKLink()
        {
            Quaternion rot = LocalRotationIKLink * IKRotation;
            LocalMatrix = Matrix4.CreateFromQuaternion(rot);
            if (Scale.X != 1f || Scale.Y != 1f || Scale.Z != 1f)
            {
                LocalMatrix.M11 = LocalMatrix.M11 * Scale.X;
                LocalMatrix.M12 = LocalMatrix.M12 * Scale.X;
                LocalMatrix.M13 = LocalMatrix.M13 * Scale.X;
                LocalMatrix.M21 = LocalMatrix.M21 * Scale.Y;
                LocalMatrix.M22 = LocalMatrix.M22 * Scale.Y;
                LocalMatrix.M23 = LocalMatrix.M23 * Scale.Y;
                LocalMatrix.M31 = LocalMatrix.M31 * Scale.Z;
                LocalMatrix.M32 = LocalMatrix.M32 * Scale.Z;
                LocalMatrix.M33 = LocalMatrix.M33 * Scale.Z;
            }
            LocalMatrix.M41 = LocalMatrix.M41 + LocalTranslationIKLink.X;
            LocalMatrix.M42 = LocalMatrix.M42 + LocalTranslationIKLink.Y;
            LocalMatrix.M43 = LocalMatrix.M43 + LocalTranslationIKLink.Z;
            BoneMatrix = LocalMatrix;
            LocalMatrix.M41 = LocalMatrix.M41 + InitialOffset.X;
            LocalMatrix.M42 = LocalMatrix.M42 + InitialOffset.Y;
            LocalMatrix.M43 = LocalMatrix.M43 + InitialOffset.Z;
            if (Parent != null)
            {
                LocalMatrix *= Parent.LocalMatrix;
            }
        }

        public void ResetTransform(bool link)
        {
            BoneMatrix = Matrix4.Identity;
            LocalMatrix = Matrix4.Identity;
            TransformMatrix = Matrix4.Identity;

            LocalScale = new Vector3(1f);
            LocalRotation = Vector3.Zero;

            Scale = Vector3.One;
            Rotation = Vector3.Zero;
            Translation = Vector3.Zero;

            LocalRotationIKLink = Quaternion.Identity;
            LocalTranslationIKLink = Vector3.Zero;
            if (link)
                IKRotation = Quaternion.Identity;
        }


        public void SetTransform(Vector3 scale, Vector3 rot, Vector3 trans)
        {
            Scale = scale;
            Rotation = rot;
            Translation = trans;
        }

        public void SetTransform(Vector3 rot, Vector3 trans)
        {
            Rotation = rot;
            Translation = trans;
        }

        public void UpdateTransformMatrix()
        {
            TransformMatrix = LocalSpace * LocalMatrix;
        }

        public void UpdateLocalRotation()
        {
            if (!LocalRotationFlag)
            {
                LocalMatrix = Matrix4.CreateFromQuaternion(new Quaternion(LocalRotation));
                LocalRotation.Normalize();
                LocalRotationFlag = true;
            }
        }

        public Vector3 GetTransformedBonePosition()
        {
            return new Vector3(LocalMatrix.M41, LocalMatrix.M42, LocalMatrix.M43);
        }
    }
}

using OpenTK;
using System;

namespace Toys
{
    public class BoneTransform
    {
        public readonly Bone Bone;
        public Matrix4 TransformMatrix;
        public Matrix4 PhysTransform;
        public bool Phys = false;

        //initial bone space coordinetes for reference
        public Matrix4 LocalSpaceDefault;
        //inverted initial bone space coordinetes for calculating tranform
        public Matrix4 LocalSpaceInverted;

        public Matrix4 LocalMatrix;
        Matrix4 BoneMatrix;
        public Vector3 InitialOffset;

        Vector3 Translation;
        Quaternion Rotation;
        Vector3 Scale;
        Quaternion AddRotation;
        Vector3 AddTranslation;

        Vector3 LocalTranslation;
        //Quaternion LocalRotation;
        Vector3 LocalScale;

        //IK Variables
        public Quaternion IKRotation;
        public Quaternion LocalRotationForIKLink;
        Vector3 LocalTranslationForIKLink;

        public bool IsIK;
        public bool IsIKLink;
        public bool IsAddLocal;
        public bool IsRotateAdd;
        public bool IsTranslateAdd;
        public bool PhysicsIKSkip;
        public bool LocalRotationFlag;

        public BoneTransform Parent;
        public BoneTransform AddParent;
        public float AddRatio;
        public IKResolver IK;
        
        bool changed;

        public BoneTransform(Bone b)
        {
            Bone = b;
            LocalMatrix = Matrix4.Identity;
            LocalSpaceDefault = Matrix4.Identity;
            LocalSpaceInverted = Matrix4.Identity;
            IsIK = Bone.IK;
            IKRotation = Quaternion.Identity;
            PhysTransform = Matrix4.Identity;
            ResetTransform(false);
        }

        public void UpdateLocalMatrix(bool ik = true)
        {
            Quaternion rot = Rotation;
            Vector3 trans = Translation;

            //additional parent bone
            if (IsRotateAdd && AddParent != null)
	        {
		        AddRotation = Quaternion.Identity;
		        if (!IsAddLocal)
		        {
			        AddRotation = ((!AddParent.IsRotateAdd) ? AddParent.Rotation : AddParent.AddRotation);
		        }
		        else
		        {
			        AddRotation = AddParent.LocalMatrix.ExtractRotation();
		        }
		        if (AddParent.IsIKLink && !IsAddLocal)
		        {
			        AddRotation *= AddParent.IKRotation;
		        }
		        if (AddRatio != 1f)
		        {
			        AddRotation = Quaternion.Slerp(Quaternion.Identity, AddRotation, AddRatio);
		        }
		        rot = (AddRotation *= rot);
	        }
	        if (IsTranslateAdd && AddParent != null)
	        {
		        AddTranslation = Vector3.Zero;
		        if (!IsAddLocal)
		        {
			        AddTranslation = ((!AddParent.IsTranslateAdd) ? (AddParent.Translation) : AddParent.AddTranslation);
		        }
		        else
		        {
			        AddTranslation.X = AddParent.LocalMatrix.M41 - AddParent.LocalSpaceDefault.M41;
			        AddTranslation.Y = AddParent.LocalMatrix.M42 - AddParent.LocalSpaceDefault.M42;
			        AddTranslation.Z = AddParent.LocalMatrix.M43 - AddParent.LocalSpaceDefault.M43;
		        }
		        if (AddRatio != 1f)
		        {
			        AddTranslation *= AddRatio;
		        }
		        trans = (AddTranslation += trans);
	        }


            if (IsIKLink)
            {
                LocalRotationForIKLink = rot;
                LocalTranslationForIKLink = trans;
                rot *= IKRotation;
            }

            LocalMatrix = Matrix4.CreateFromQuaternion(rot);
            
            if (Scale.X != 1f || Scale.Y != 1f || Scale.Z != 1f)
                LocalMatrix *= Matrix4.CreateScale(Scale);

            LocalMatrix.M41 += trans.X;
            LocalMatrix.M42 += trans.Y;
            LocalMatrix.M43 += trans.Z;
            BoneMatrix = LocalMatrix;
            LocalMatrix.M41 += InitialOffset.X;
            LocalMatrix.M42 += InitialOffset.Y;
            LocalMatrix.M43 += InitialOffset.Z;

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
            Quaternion rot = LocalRotationForIKLink * IKRotation;
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
            LocalMatrix.M41 += LocalTranslationForIKLink.X;
            LocalMatrix.M42 += LocalTranslationForIKLink.Y;
            LocalMatrix.M43 += LocalTranslationForIKLink.Z;
            BoneMatrix = LocalMatrix;
            LocalMatrix.M41 += InitialOffset.X;
            LocalMatrix.M42 += InitialOffset.Y;
            LocalMatrix.M43 += InitialOffset.Z;
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

            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
            Translation = Vector3.Zero;

            //IK
            LocalRotationForIKLink = Quaternion.Identity;
            LocalTranslationForIKLink = Vector3.Zero;
            if (link)
                IKRotation = Quaternion.Identity;

            AddTranslation = Vector3.Zero;
            AddRotation = Quaternion.Identity;
        }


        public void SetTransform(Vector3 scale, Quaternion rot, Vector3 trans)
        {
            Scale = scale;
            Rotation = rot;
            Translation = trans;
        }

        public void SetTransform(Quaternion rot, Vector3 trans)
        {
            Rotation = rot;
            Translation = trans;
        }

        public void UpdateTransformMatrix()
        {            
            if (!Phys){
                /*
                BoneMatrix = LocalSpaceInverted * BoneMatrix * LocalSpaceDefault;
                TransformMatrix = (Parent == null) ? BoneMatrix : BoneMatrix * Parent.TransformMatrix;
                */
                TransformMatrix = LocalSpaceInverted * LocalMatrix;
            }
            else 
            {
                TransformMatrix = PhysTransform;
                Phys = false;
            }
            
        }

        /*
        public void UpdateLocalRotation()
        {
            if (!LocalRotationFlag)
            {
                LocalMatrix = Matrix4.CreateFromQuaternion(LocalRotation);
                LocalRotation.Normalize();
                LocalRotationFlag = true;
            }
        }
        */

        public Vector3 GetTransformedBonePosition()
        {
            return new Vector3(LocalMatrix.M41, LocalMatrix.M42, LocalMatrix.M43);
        }
    }
}

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
        Matrix4 boneSpaceTranstorm;
        public Matrix4 World2BoneInitial {
            get { return boneSpaceTranstorm; }
            internal set
            {
                boneSpaceTranstorm = value;
                Bone2WorldInitial = boneSpaceTranstorm.Inverted();
            }
        }
        //inverted initial bone space coordinetes for calculating tranform
        public Matrix4 Bone2WorldInitial { get; private set; }

        //fully transformed matrix
        public Matrix4 LocalMatrix;
        //current bone transform matrix
        internal Matrix4 BoneMatrix;
        public Matrix4 InitialLocalTransform { get; internal set; }
        //public Vector3 InitialOffset;

        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }
        Vector3 Scale;
        Quaternion AddRotation;
        Vector3 AddTranslation;

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

        public BoneTransform(Bone bone)
        {
            Bone = bone;
            LocalMatrix = Matrix4.Identity;
            World2BoneInitial = Matrix4.Identity;
            Bone2WorldInitial = Matrix4.Identity;
            IsIK = Bone.IK;
            IKRotation = Quaternion.Identity;
            PhysTransform = Matrix4.Identity;
            InitialLocalTransform = bone.Parent2Local;
            ResetTransform(false);
        }

        public void UpdateLocalMatrix(bool ik = true)
        {
            Quaternion rot = Rotation;
            Vector3 trans = Translation;

            //additional parent bone
            if (IsRotateAdd && AddParent != null)
	        {
		        var addRotation = Quaternion.Identity;
		        if (!IsAddLocal)
		        {
			        addRotation = ((!AddParent.IsRotateAdd) ? AddParent.Rotation : AddParent.AddRotation);
		        }
		        else
		        {
                    addRotation = AddParent.LocalMatrix.ExtractRotation();
		        }
		        if (AddParent.IsIKLink && !IsAddLocal)
		        {
                    addRotation = AddParent.IKRotation * addRotation;
		        }
		        if (AddRatio != 1f)
		        {
                    addRotation = Quaternion.Slerp(Quaternion.Identity, AddRotation, AddRatio);
		        }
		        rot = (AddRotation = rot * addRotation);
	        }
	        if (IsTranslateAdd && AddParent != null)
	        {
		        AddTranslation = Vector3.Zero;
		        if (!IsAddLocal)
		        {
			        AddTranslation = (!AddParent.IsTranslateAdd) ? (AddParent.Translation) : AddParent.AddTranslation;
		        }
		        else
		        {
			        AddTranslation.X = AddParent.LocalMatrix.M41 - AddParent.World2BoneInitial.M41;
			        AddTranslation.Y = AddParent.LocalMatrix.M42 - AddParent.World2BoneInitial.M42;
			        AddTranslation.Z = AddParent.LocalMatrix.M43 - AddParent.World2BoneInitial.M43;
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
            if (Parent != null)
            {

                LocalMatrix *= InitialLocalTransform;
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
            Quaternion rot = IKRotation * LocalRotationForIKLink;
            LocalMatrix = Matrix4.CreateFromQuaternion(rot);
            if (Scale.X != 1f || Scale.Y != 1f || Scale.Z != 1f)
            {
                LocalMatrix.M11 *= Scale.X;
                LocalMatrix.M12 *= Scale.X;
                LocalMatrix.M13 *= Scale.X;
                LocalMatrix.M21 *= Scale.Y;
                LocalMatrix.M22 *= Scale.Y;
                LocalMatrix.M23 *= Scale.Y;
                LocalMatrix.M31 *= Scale.Z;
                LocalMatrix.M32 *= Scale.Z;
                LocalMatrix.M33 *= Scale.Z;
            }
            LocalMatrix.M41 += LocalTranslationForIKLink.X;
            LocalMatrix.M42 += LocalTranslationForIKLink.Y;
            LocalMatrix.M43 += LocalTranslationForIKLink.Z;
            BoneMatrix = LocalMatrix;
            LocalMatrix *= InitialLocalTransform;
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


        public void SetTransform(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            Scale = scale;
            Rotation = rotation;
            Translation = translation;
        }

        public void SetTransform(Quaternion rotation, Vector3 translation)
        {
            Rotation = rotation;
            Translation = translation;
        }

        public void SetTransform(Quaternion rotation)
        {
            Rotation = rotation;
        }

        public void SetTransform(Vector3 translation)
        {
            Translation = translation;
        }

        public void UpdateTransformMatrix()
        {            
            if (!Phys){
                TransformMatrix = Bone2WorldInitial * LocalMatrix;
            }
            else 
            {
                TransformMatrix = PhysTransform;
                //Phys = false;
            }       
        }

        public Vector3 GetTransformedBonePosition()
        {
            return new Vector3(LocalMatrix.M41, LocalMatrix.M42, LocalMatrix.M43);
        }
    }
}

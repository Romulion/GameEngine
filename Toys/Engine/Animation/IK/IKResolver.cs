/*
 * TODO
 * Figure out how interpolation works
 */
using System;
using OpenTK;

namespace Toys
{
    public class IKResolver
    {
        private bool _isEnable;
        private bool[] _fixAxis;
        public float LimitOnce;
        public int LoopCount;

        private Vector3 _ikPosition;

        private Vector3 _targetPosition;

        public bool IsEnable
        {
            get
            {
                return _isEnable;
            }
            set
            {
                _isEnable = value;
            }
        }

        public BoneTransform IK {get; private set;}

        public BoneTransform Target {get; private set;}

        public BoneTransform[] IKLinksBones { get; private set;}

        public IKLink[] IKLinks { get; private set;}

        public bool ContainLimit
        {
            get;
            private set;
        }

        public bool IsPhysicsAllLink { get; set; }

        public IKResolver()
        {
            IsEnable = false;
            LimitOnce = (float)Math.PI;
            LoopCount = 1;
            IsPhysicsAllLink = false;
        }

        public IKResolver(BoneTransform[] arr, int id) : this()
		{
            Initialize(arr, id);
        }

        public void ClearLinks()
        {
            if (IKLinks != null)
                IKLinks = null;
        }

        public bool Initialize(BoneTransform[] arr, int id)
        {
            if (!arr[id].IsIK)
                return false;

            IK = arr[id];
            BoneIK iK = IK.Bone.IKData;

            IKLinksBones = new BoneTransform[iK.Links.Length];
            _fixAxis = new bool[iK.Links.Length];
            IKLinks = new IKLink[iK.Links.Length];
            ContainLimit = false;

            for (int i = 0; i < iK.Links.Length; i++)
            {
                if (arr.Length > iK.Links[i].Bone && arr.Length >= 0)
                {
                    IKLinks[i] = iK.Links[i];
                    IKLinksBones[i] = arr[iK.Links[i].Bone];
                    IKLinksBones[i].IsIKLink = true;
                    if (IKLinks[i].IsLimit)
                    {
                        ContainLimit = true;
                        IKLinks[i].NormalizeAngle();
                        IKLinks[i].NormalizeEulerAxis();
                        if (IKLinks[i].FixAxis == IKLink.FixAxisType.Fix)
                            _fixAxis[i] = true;
                    }
                }
            }

            LimitOnce = iK.AngleLimit;
            LoopCount = Math.Min(iK.LoopCount, 256);

            if (arr.Length > iK.Target && iK.Target >=0)
            {
                Target = arr[iK.Target];
                IK.IsIK = true;
                IsEnable = true;
                return true;
            }
            Target = null;
            return false;
        }

        public void InitializeAngle()
        {
            for (int i = 0; i < IKLinksBones.Length; i++)
            {
                IKLinksBones[i].IKRotation = Quaternion.Identity;
            }
        }

        private void CalcBonePosition(int link)
        {
            for (int i = link; i >= 0; i--)
            {
                IKLinksBones[i].UpdateLocalMatrix(false);
            }
            Target.UpdateLocalMatrix(false);
            _targetPosition = Target.GetTransformedBonePosition();
        }

        private void CalcBonePosition_Link(int link)
        {
            for (int i = link; i >= 0; i--)
            {
                IKLinksBones[i].UpdateLocalMatrixIKLink();
            }
            Target.UpdateLocalMatrix(false);
            _targetPosition = Target.GetTransformedBonePosition();
        }

        public void Transform()
        {
            if (!IsEnable || Target == null || IKLinksBones.Length == 0)
                return;
            InitializeAngle();
            CalcBonePosition(IKLinksBones.Length - 1);
            _ikPosition = IK.GetTransformedBonePosition();
            Target.UpdateLocalMatrix();
            _targetPosition = Target.GetTransformedBonePosition();
            
            if ((_ikPosition - _targetPosition).LengthSquared < 1E-08f)
                return;

            int num = LoopCount / 2;
            for (int i = 0; i < LoopCount; i++)
            {
                for (int j = 0; j < IKLinksBones.Length; j++)
                {
                    if (!_fixAxis[j])
                    {
                        IKProc_Link(j, i < num);
                    }
                }
                if ((_ikPosition - _targetPosition).LengthSquared < 1E-08f)
                    break;
            }
        }

        private void IKProc_Link(int linkNum, bool axis_lim = true)
        {   
            BoneTransform transformBone = IKLinksBones[linkNum];

            Vector3 linkPos = transformBone.GetTransformedBonePosition();
            Vector3 vectLinkTarget = linkPos - _targetPosition;
            vectLinkTarget.Normalize();
            Vector3 vectLinkIK = linkPos - _ikPosition;
            vectLinkIK.Normalize();

            Matrix4 matrixParent = (transformBone.Parent == null) ? Matrix4.Identity : transformBone.Parent.LocalMatrix;

            //matrix invertion
            matrixParent.M41 = (matrixParent.M42 = (matrixParent.M43 = 0f));
            Matrix4 matrix2 = Matrix4.Transpose(matrixParent);
            matrix2.M14 = (matrix2.M24 = (matrix2.M34 = 0f));
            //finding axis to rotate
            Vector3 axis = Vector3.Cross(vectLinkTarget, vectLinkIK);

            //converting axis to local space
            if (IKLinks[linkNum].IsLimit & axis_lim)
            {
                switch (IKLinks[linkNum].FixAxis)
                {
                    case IKLink.FixAxisType.None:
                        {
                            axis = Vector3.TransformNormal(axis, matrix2);
                            axis.Normalize();
                            break;
                        }
                    case IKLink.FixAxisType.X:
                        {
                            float num = Vector3.Dot(axis, new Vector3(matrixParent.M11, matrixParent.M12, matrixParent.M13));
                            axis.X = ((num >= 0f) ? 1 : -1);
                            axis.Y = (axis.Z = 0f);
                            break;
                        }
                    case IKLink.FixAxisType.Y:
                        {
                            float num = Vector3.Dot(axis, new Vector3(matrixParent.M21, matrixParent.M22, matrixParent.M23));
                            axis.Y = ((num >= 0f) ? 1 : -1);
                            axis.X = (axis.Z = 0f);
                            break;
                        }
                    case IKLink.FixAxisType.Z:
                        {
                            float num = Vector3.Dot(axis, new Vector3(matrixParent.M31, matrixParent.M32, matrixParent.M33));
                            axis.Z = ((num >= 0f) ? 1 : -1);
                            axis.X = (axis.Y = 0f);
                            break;
                        }
                }
            }
            else
            {
                axis = Vector3.TransformNormal(axis,matrix2);
                axis.Normalize();
            }

            float dot = Vector3.Dot(vectLinkTarget, vectLinkIK);
            if (dot > 1f)
            {
                dot = 1f;
            }
            else if (dot < -1f)
            {
                dot = -1f;
            }

            float angleTargetIK = (float)Math.Acos(dot);
            float angleMaxRotation = LimitOnce * (linkNum + 1);
            angleTargetIK = Math.Min(angleTargetIK, angleMaxRotation);

            //prevent paralel vectors error 
            if (angleTargetIK != 0 && !Single.IsNaN(axis.X))
                transformBone.IKRotation = Quaternion.FromAxisAngle(axis, angleTargetIK) * transformBone.IKRotation;
  
            
            if (IKLinks[linkNum].IsLimit)
            {
                Vector3 angle = Vector3.Zero;
                Matrix4 matrixIKRotation = Matrix4.CreateFromQuaternion(transformBone.LocalRotationForIKLink * transformBone.IKRotation);

                //limit angles from rotation matrix acording to rotation sequence
                switch (IKLinks[linkNum].Euler)
                {
                    case IKLink.EulerType.ZXY:
                        {
                            angle.X = (float)Math.Asin(-matrixIKRotation.M32);
                            if (Math.Abs(angle.X) > 1.535889f)
                            {
                                angle.X = ((angle.X < 0f) ? -1.535889f : 1.535889f);
                            }
                            angle.Y = (float)Math.Atan2(matrixIKRotation.M31, matrixIKRotation.M33);
                            angle.Z = (float)Math.Atan2(matrixIKRotation.M12, matrixIKRotation.M22);
                            LimitAngle(ref angle, linkNum, axis_lim);
                            transformBone.IKRotation = Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.X) * Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z);
                            break;
                        }
                    case IKLink.EulerType.XYZ:
                        {
                            angle.Y = (float)Math.Asin(-matrixIKRotation.M13);
                            if (Math.Abs(angle.Y) > 1.535889f)
                            {
                                angle.Y = ((angle.Y < 0f) ? -1.535889f : 1.535889f);
                            }
                            angle.X = (float)Math.Atan2(matrixIKRotation.M23, matrixIKRotation.M33);
                            angle.Z = (float)Math.Atan2(matrixIKRotation.M12, matrixIKRotation.M11);
                            LimitAngle(ref angle, linkNum, axis_lim);
                            transformBone.IKRotation =  Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.X);
                            break;
                        }
                    case IKLink.EulerType.YZX:
                        {
                            angle.Z = (float)Math.Asin(-matrixIKRotation.M21);
                            if (Math.Abs(angle.Z) > 1.535889f)
                            {
                                angle.Z = ((angle.Z < 0f) ? -1.535889f : 1.535889f);
                            }
                            angle.X = (float)Math.Atan2(matrixIKRotation.M23, matrixIKRotation.M22);
                            angle.Y = (float)Math.Atan2(matrixIKRotation.M31, matrixIKRotation.M11);
                            LimitAngle(ref angle, linkNum, axis_lim);
                            transformBone.IKRotation = Quaternion.FromAxisAngle(Vector3.UnitX, angle.X) * Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y);
                            break;
                        }
                }
                transformBone.IKRotation = Quaternion.Invert(transformBone.LocalRotationForIKLink) * transformBone.IKRotation;
            }

            CalcBonePosition_Link(linkNum);
        }

        private void LimitAngle(ref Vector3 angle, int linkNum, bool axis_lim)
        {
            Vector3 high = IKLinks[linkNum].LimitMax;
            Vector3 low = IKLinks[linkNum].LimitMin;
            if (angle.X < low.X)
            {
                float num = 2f * low.X - angle.X;
                angle.X = ((num <= high.X & axis_lim) ? num : low.X);
            }
            else if (angle.X > high.X)
            {
                float num = 2f * high.X - angle.X;
                angle.X = ((num >= low.X & axis_lim) ? num : high.X);
            }
            if (angle.Y < low.Y)
            {
                float num = 2f * low.Y - angle.Y;
                angle.Y = ((num <= high.Y & axis_lim) ? num : low.Y);
            }
            else if (angle.Y > high.Y)
            {
                float num = 2f * high.Y - angle.Y;
                angle.Y = ((num >= low.Y & axis_lim) ? num : high.Y);
            }
            if (angle.Z < low.Z)
            {
                float num = 2f * low.Z - angle.Z;
                angle.Z = ((num <= high.Z & axis_lim) ? num : low.Z);
                return;
            }
            if (angle.Z > high.Z)
            {
                float num = 2f * high.Z - angle.Z;
                angle.Z = ((num >= low.Z & axis_lim) ? num : high.Z);
            }
        }
    }
}

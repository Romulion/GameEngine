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
        private bool enable;
        private bool[] m_fixAxis;
        public float LimitOnce;
        public int LoopCount;

        private Vector3 m_ikPosition;

        private Vector3 m_targetPosition;

        private const float ZeroValue = 0.0001f;

        private const float ZeroValue2 = 1E-08f;

        public bool Enable
        {
            get
            {
                return enable;
            }
            set
            {
                enable = value;
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
            Enable = false;
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
            m_fixAxis = new bool[iK.Links.Length];
            IKLinks = new IKLink[iK.Links.Length];
            ContainLimit = false;

            for (int i = 0; i < iK.Links.Length; i++)
            {
                if (arr.Length > iK.Links[i].Bone)
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
                            m_fixAxis[i] = true;
                    }
                }
            }

            LimitOnce = iK.AngleLimit;
            LoopCount = Math.Min(iK.LoopCount, 256);

            if (arr.Length > iK.Target)
            {
                Target = arr[iK.Target];
                IK.IsIK = true;
                Enable = true;
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
            m_targetPosition = Target.GetTransformedBonePosition();
        }

        private void CalcBonePosition_Link(int link)
        {
            for (int i = link; i >= 0; i--)
            {
                IKLinksBones[i].UpdateLocalMatrixIKLink();
            }
            Target.UpdateLocalMatrix(false);
            m_targetPosition = Target.GetTransformedBonePosition();
        }

        public void Transform()
        {
            if (!Enable || Target == null || IKLinksBones.Length == 0)
                return;

            InitializeAngle();
            CalcBonePosition(IKLinksBones.Length - 1);

            m_ikPosition = IK.GetTransformedBonePosition();
            Target.UpdateLocalMatrix(true);
            m_targetPosition = Target.GetTransformedBonePosition();
            /*
            if (IK.Bone.Index == 165)
            {
                Console.WriteLine("IK");
                Console.WriteLine(m_targetPosition);
                Console.WriteLine(m_ikPosition);
                Console.WriteLine((m_targetPosition - m_ikPosition).LengthSquared);
            }
            */
            if ( (m_ikPosition - m_targetPosition).LengthSquared < 1E-08f)
                return;

            CalcBonePosition(IKLinksBones.Length - 1);
            if ((m_ikPosition - m_targetPosition).LengthSquared < 1E-08f)
                return;

            int loopCount = LoopCount;
            int num = loopCount / 2;
            for (int i = 0; i < loopCount; i++)
            {
                for (int j = 0; j < IKLinksBones.Length; j++)
                {
                    if (!m_fixAxis[j])
                    {
                        IKProc_Link(j, i < num);
                    }
                }
                
                if (IK.Bone.Index == 165)
                {
                   // Console.WriteLine("iter end");
                   // Console.WriteLine(i < num);
                   // Console.WriteLine((m_ikPosition - m_targetPosition).LengthSquared);
                }
                
                if ((m_ikPosition - m_targetPosition).LengthSquared < 1E-08f)
                    break;
            }
        }

        private void IKProc_Link(int linkNum, bool axis_lim = true)
        {
            

            
            BoneTransform transformBone = IKLinksBones[linkNum];
            /*
            if (IK.Bone.Index == 165 && transformBone.Bone.Index == 163)
            {
                Console.WriteLine("link");
                Console.WriteLine(m_targetPosition);
                Console.WriteLine(m_ikPosition);
                Console.WriteLine((m_targetPosition - m_ikPosition).LengthSquared);
            }
            */

            Vector3 linkPos = new Vector3(transformBone.LocalMatrix.M41, transformBone.LocalMatrix.M42, transformBone.LocalMatrix.M43);
            Vector3 vectLinkTarget = linkPos - m_targetPosition;
            vectLinkTarget.Normalize();
            Vector3 vectLinkIK = linkPos - m_ikPosition;
            vectLinkIK.Normalize();
            Matrix4 matrixParent = (transformBone.Parent == null) ? Matrix4.Identity : transformBone.Parent.LocalMatrix;
            //removing transponation
            matrixParent.M41 = (matrixParent.M42 = (matrixParent.M43 = 0f));

            Matrix4 matrix2 = Matrix4.Transpose(matrixParent);
            matrix2.M14 = (matrix2.M24 = (matrix2.M34 = 0f));

            //finding axis to rotate
            Vector3 axis = Vector3.Cross(vectLinkTarget, vectLinkIK);
            
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
                axis = Vector3.TransformNormal(axis, matrix2);
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
            float angleMaxRotation = LimitOnce * (float)(linkNum + 1);
            angleTargetIK = Math.Min(angleTargetIK, angleMaxRotation);
            /*
            if (IK.Bone.Index == 165)
            {
                Console.WriteLine("angle");
                Console.WriteLine(angleTargetIK);           
            }
            */
            //prevent paralel vectors error 
            if (angleTargetIK != 0 && !Single.IsNaN(axis.X))
                transformBone.IKRotation *= Quaternion.FromAxisAngle(axis, angleTargetIK);

            if (IK.Bone.Index == 165 && Single.IsNaN(transformBone.IKRotation.X))
            {
                Console.WriteLine("ERR");
                Console.WriteLine(vectLinkTarget);
                Console.WriteLine(vectLinkIK);
                Console.WriteLine(axis);
                Console.WriteLine(angleTargetIK);
            }
            if (IK.Bone.Index == 165)
            {
                //Console.WriteLine(transformBone.IKRotation);
            }

            if (IKLinks[linkNum].IsLimit)
            {
                Vector3 angle = Vector3.Zero;
                Matrix4 matrixIKRotation = Matrix4.CreateFromQuaternion(transformBone.LocalRotationForIKLink * transformBone.IKRotation);

                //determine angles from rotation matrix acording to rotation sequence
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
                            transformBone.IKRotation = Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.X) * Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y);
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
                            transformBone.IKRotation = Quaternion.FromAxisAngle(Vector3.UnitX, angle.X) * Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y) * Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z);
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
                            transformBone.IKRotation = Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y) * Quaternion.FromAxisAngle(Vector3.UnitZ, angle.Z) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.X);
                            break;
                        }
                }
                transformBone.IKRotation = Quaternion.Invert(transformBone.LocalRotationForIKLink) * transformBone.IKRotation;
            }
            CalcBonePosition_Link(linkNum);
        }

        //normalize angles to limit
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
                float num2 = 2f * high.X - angle.X;
                angle.X = ((num2 >= low.X & axis_lim) ? num2 : high.X);
            }
            if (angle.Y < low.Y)
            {
                float num3 = 2f * low.Y - angle.Y;
                angle.Y = ((num3 <= high.Y & axis_lim) ? num3 : low.Y);
            }
            else if (angle.Y > high.Y)
            {
                float num4 = 2f * high.Y - angle.Y;
                angle.Y = ((num4 >= low.Y & axis_lim) ? num4 : high.Y);
            }
            if (angle.Z < low.Z)
            {
                float num5 = 2f * low.Z - angle.Z;
                angle.Z = ((num5 <= high.Z & axis_lim) ? num5 : low.Z);
                return;
            }
            if (angle.Z > high.Z)
            {
                float num6 = 2f * high.Z - angle.Z;
                angle.Z = ((num6 >= low.Z & axis_lim) ? num6 : high.Z);
            }
        }

        Vector3 GetLineCrossPoint(ref Vector3 p, ref Vector3 from, ref Vector3 dir, out float d)
        {
            Vector3 vector = p - from;
            d = Vector3.Dot(dir, vector);
            return dir * d + from;
        }

        Vector3 GetLineCrossPoint(ref Vector3 p, ref Vector3 from, ref Vector3 dir)
        {
            float num;
            return GetLineCrossPoint(ref p, ref from, ref dir, out num);
        }
    }
}

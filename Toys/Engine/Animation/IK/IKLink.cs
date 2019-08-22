using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class IKLink
    {
        public int Bone;
        public bool IsLimit;
        public Vector3 LimitMin;
        public Vector3 LimitMax;

        public EulerType Euler;
        public FixAxisType FixAxis;

        public enum EulerType
        {
            ZXY,
            XYZ,
            YZX
        }

        public enum FixAxisType
        {
            None,
            Fix,
            X,
            Y,
            Z
        }

        public void NormalizeAngle()
        {
            Vector3 low;
            Vector3 high;

            low.X = Math.Min(LimitMin.X, LimitMax.X);
            high.X = Math.Max(LimitMin.X, LimitMax.X);
            low.Y = Math.Min(LimitMin.Y, LimitMax.Y);
            high.Y = Math.Max(LimitMin.Y, LimitMax.Y);
            low.Z = Math.Min(LimitMin.Z, LimitMax.Z);
            high.Z = Math.Max(LimitMin.Z, LimitMax.Z);
            LimitMin = low;
            LimitMax = high;
        }

        public void NormalizeEulerAxis()
        {
            float pi05 = (float)Math.PI / 2;
            if (-pi05 < LimitMin.X && LimitMax.X < pi05)
            {
                Euler = EulerType.ZXY;
            }
            else if (-pi05 < LimitMin.Y && LimitMax.Y < pi05)
            {
                Euler = EulerType.XYZ;
            }
            else
            {
                Euler = EulerType.YZX;
            }

            FixAxis = FixAxisType.None;

            if (LimitMin.X == 0f && LimitMax.X == 0f && LimitMin.Y == 0f && LimitMax.Y == 0f && LimitMin.Z == 0f && LimitMax.Z == 0f)
            {
                FixAxis = FixAxisType.Fix;
            }
            else if (LimitMin.Y == 0f && LimitMax.Y == 0f && LimitMin.Z == 0f && LimitMax.Z == 0f)
            {
                FixAxis = FixAxisType.X;
            }
            else if (LimitMin.X == 0f && LimitMax.X == 0f && LimitMin.Z == 0f && LimitMax.Z == 0f)
            {
                FixAxis = FixAxisType.Y;
            }
            else if (LimitMin.X == 0f && LimitMax.X == 0f && LimitMin.Y == 0f && LimitMax.Y == 0f)
            {
                FixAxis = FixAxisType.Z;
            }
        }
    }
}

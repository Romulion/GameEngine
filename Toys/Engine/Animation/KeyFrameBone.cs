using OpenTK.Mathematics;

namespace Toys
{
    public class KeyFrameBone
    {
        public float FrameId;
        public BonePosition BonePosition;

        //Bezier curves data
        //1 - previous frame
        //2 - this frame
        public Vector2 CurveX1;
        public Vector2 CurveX2;
        public Vector2 CurveY1;
        public Vector2 CurveY2;
        public Vector2 CurveZ1;
        public Vector2 CurveZ2;
        public Vector2 CurveR1;
        public Vector2 CurveR2;

        public BezierCurveCubic bezierX;
        public BezierCurveCubic bezierY;
        public BezierCurveCubic bezierZ;
        public BezierCurveCubic bezierR;
        public bool InterpolateCurve = false;

        public KeyFrameBone(float frame, BonePosition boneposition)
        {
            FrameId = frame;
            BonePosition = boneposition;
        }
    }
    public class BonePosition
    {
        public readonly Vector3 Scale;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector4 RotationVec;
        public readonly int BoneId;

        public BonePosition(Vector3 position, Quaternion rotation,int id)
        {
            Scale = Vector3.One;
            Position = position;
            Rotation = rotation;
            BoneId = id;
        }

        public BonePosition(Vector3 position, Quaternion rotation, Vector3 scale, int id)
        {
            Scale = scale;
            Position = position;
            Rotation = rotation;
            BoneId = id;
        }

        public BonePosition(Vector3 position, Vector4 rotation, int id)
        {
            Scale = Vector3.One;
            Position = position;
            RotationVec = rotation;
            BoneId = id;
        }
    }
}

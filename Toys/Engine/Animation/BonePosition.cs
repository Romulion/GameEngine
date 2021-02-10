using OpenTK.Mathematics;

namespace Toys
{
    internal struct BonePosition
    {
        internal readonly Vector3 Position;
        internal readonly Vector4 Rotation;
        internal readonly int BoneId;

        internal BonePosition(Vector3 position, Vector4 rotation,int id)
        {
            Position = position;
            Rotation = rotation;
            BoneId = id;
        }
    }
}

using OpenTK;

namespace Toys
{
    internal struct BonePosition
    {
        internal readonly Vector3 position;
        internal readonly Vector4 rotation;
        internal readonly int boneId;

        internal BonePosition(Vector3 pos, Vector4 rot,int id)
        {
            position = pos;
            rotation = rot;
            boneId = id;
        }
    }
}

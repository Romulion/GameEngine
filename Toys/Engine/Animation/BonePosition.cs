using OpenTK;

namespace Toys
{
    internal struct BonePosition
    {
        internal readonly Vector3 position;
        internal readonly Vector3 rotation;
        internal int boneId;
        internal readonly string BoneName;

        internal BonePosition(Vector3 pos, Vector3 rot,string Name)
        {
            position = pos;
            rotation = rot;
            BoneName = Name;
            boneId = 0;
        }
    }
}

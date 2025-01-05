using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    internal class AssimpNodeTransform
    {
        public string Name;
        public bool IsBone;
        public Matrix4 World2Bone;
        public Matrix4 Parent2Bone;
        public Matrix4 LocalTransform = Matrix4.Identity;
    }
}

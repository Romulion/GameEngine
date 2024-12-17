using System;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Toys
{
    /// <summary>
    /// Vertex rigged.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct VertexRigged3D 
	{
        [FieldOffset(0)]
        public Vector3 Position;
        [FieldOffset(16)]
        public Vector3 Normal;
        [FieldOffset(32)]
        public Vector2 UV;
        [FieldOffset(48)]
        public IVector4 BoneIndices;
        [FieldOffset(64)]
        public Vector4 BoneWeigths;


		public VertexRigged3D(Vector3 postion, Vector3 normal, Vector2 uv, IVector4 boneIdices, Vector4 boneWeigth)
		{
			Position = postion;
			Normal = normal;
			UV = uv;
			BoneIndices = boneIdices;
			BoneWeigths = boneWeigth;
		}

		public static explicit operator Vertex3D(VertexRigged3D rigged)
		{
			return new Vertex3D(rigged.Position,rigged.Normal,rigged.UV);
		}
	
	}

	public struct IVector4
	{
		public int bone1;
		public int bone2;
		public int bone3;
		public int bone4;

		public IVector4(int[] bones)
		{
			bone1 = bones[0];
			bone2 = bones[1];
			bone3 = bones[2];
			bone4 = bones[3];
		}

		public override string ToString()
		{
			return string.Format("({0},{1},{2},{3})",bone1,bone2,bone3,bone4);
		}

		public int this[int i]
		{
			get
			{
					
				switch (i)
				{
					case 0:
						return bone1;
					case 1:
						return bone2;
					case 2:
						return bone3;
					case 3:
						return bone4;
				}

				return 0;
			}
			set
			{
                switch (i)
                {
                    case 0:
                        bone1 = value;
						break;
                    case 1:
                        bone2 = value;
                        break;
                    case 2:
                        bone3 = value;
                        break;
                    case 3:
                        bone4 = value;
                        break;
                }
            }
		}
	}

}

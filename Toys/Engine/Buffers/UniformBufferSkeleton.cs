using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Toys
{
	public class UniformBufferSkeleton: UniformBuffer
	{
		const int maxBonesCount = 700;
		const int boneMatrixSize = 64;
		const string name = "skeleton";

		public UniformBufferSkeleton(int bindingPoint) : base(maxBonesCount * boneMatrixSize, name, bindingPoint)
		{
		}

		public void SetBones(Matrix4[] mat)
		{
			//restrict skeleton size
			int count = (mat.Length > maxBonesCount) ? maxBonesCount : mat.Length;

			SetMatrixArray(mat, 0);
		}


	}
}

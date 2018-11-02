using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Toys
{
	public class UniformBufferSkeleton: UniformBuffer
	{
		const int maxBonesCount = 500;
		const int boneMatrixSize = 64;
		const string name = "skeleton";
		const int bindingPoint = 0;


		public UniformBufferSkeleton() : base(maxBonesCount * boneMatrixSize, name, bindingPoint)
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

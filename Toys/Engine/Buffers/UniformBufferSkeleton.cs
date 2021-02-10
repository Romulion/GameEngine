using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Toys
{
	public class UniformBufferSkeleton: UniformBuffer
	{
		const int boneMatrixSize = 64;
		const string name = "skeleton";
		int maxBonesCount;

		public UniformBufferSkeleton(int bindingPoint, int bonesCount) : base(bonesCount * boneMatrixSize, name, bindingPoint)
		{
			maxBonesCount = bonesCount;
		}

		public void SetBones(Matrix4[] mat)
		{
			//restrict skeleton size
			int count = (mat.Length > maxBonesCount) ? maxBonesCount : mat.Length;

			SetMatrixArray(mat, 0);
		}


	}
}

using System;
using OpenTK;

namespace Toys
{
	public class UniformBufferLight : UniformBuffer
	{
		const int defaultAligment = 16;
		const int size = 4 * defaultAligment;
		public UniformBufferLight() : base (size, "light", 2)
		{
		}

		public void SetLightPos(Vector3 vec)
		{
			SetVector3(vec, 0);
		}

		public void SetViewPos(Vector3 vec)
		{
			SetVector3(vec, defaultAligment);
		}

		public void SetNearPlane(float val)
		{
			SetFloat(val, 2 * defaultAligment);
		}

		public void SetFarPlane(float val)
		{
			SetFloat(val, 3 * defaultAligment);
		}
	}
}

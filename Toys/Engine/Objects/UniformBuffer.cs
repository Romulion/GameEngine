using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Toys
{
	public class UniformBuffer
	{
		readonly int UBO;
		readonly string name;

		public UniformBuffer(int size,string bindingName)
		{
			UBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferData(BufferTarget.UniformBuffer, size, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			//adding binding to existing shaders
			name = bindingName;
			ShaderManager.GetInstance.SetBinding(0, bindingName);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, UBO);
		}

		//rebinding buffer to new shaders
		public void Rebind()
		{
			ShaderManager.GetInstance.SetBinding(0, name);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, UBO);
		}

		//setting values matrices
		public void SetMatrix(Matrix4 mat)
		{
        	GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref mat);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}


		public void SetBones(Matrix4[] mat)
		{
			int size = Marshal.SizeOf(typeof(Matrix4));
			//restrict skeleton size
			int count = (mat.Length > 300) ? 300 : mat.Length;
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, count * size, mat);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

	}
}

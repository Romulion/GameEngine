using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;

namespace Toys
{
	/// <summary>
	/// skeleton index 0
	/// model index 1
	/// 
	/// </summary>
	public class UniformBuffer
	{
		protected readonly int UBO;
		readonly string name;
		public readonly int bufferIndex;

		public UniformBuffer(int size, string bindingName, int bindingPoint)
		{
			bufferIndex = bindingPoint;

			UBO = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferData(BufferTarget.UniformBuffer, size, IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);

			//adding binding to existing shaders
			name = bindingName;
			ShaderManager.GetInstance.SetBinding(bufferIndex, bindingName);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bufferIndex, UBO);
		}

		//binding buffer to shaders
		public void Rebind()
		{
			ShaderManager.GetInstance.SetBinding(bufferIndex, name);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bufferIndex, UBO);
		}

		//setting values matrices
		public void SetMatrix(Matrix4 mat, int offset)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, Marshal.SizeOf(typeof(Matrix4)), ref mat);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		public void SetVector4(Vector4 vec, int offset)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, Marshal.SizeOf(typeof(Vector4)), ref vec);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		public void SetVector3(Vector3 vec, int offset)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, Marshal.SizeOf(typeof(Vector3)), ref vec);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		public void SetVector2(Vector2 vec, int offset)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, Marshal.SizeOf(typeof(Vector2)), ref vec);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}
		public void SetFloat(float fl, int offset)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, Marshal.SizeOf(typeof(float)), ref fl);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}


		public void SetMatrixArray(Matrix4[] mat, int offset)
		{
			int size = Marshal.SizeOf(typeof(Matrix4));

			GL.BindBuffer(BufferTarget.UniformBuffer, UBO);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, mat.Length * size, mat);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

	}
}

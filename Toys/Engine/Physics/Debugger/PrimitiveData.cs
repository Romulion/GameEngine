using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class PrimitiveData : IDisposable
	{
		public int VertexCount;
		public int ElementCount;

		public int VertexBufferID;
		public int ElementBufferID;
		public DrawElementsType ElementsType;
		public PrimitiveType PrimitiveType = PrimitiveType.Triangles;

		public List<InstanceData> Instances = new List<InstanceData>();




		public void SetVertexBuffer<T>(T[] vertices) where T : struct
		{
			SetBuffer(vertices, out VertexBufferID);
		}


		public void SetDynamicVertexBuffer<T>(T[] vertices) where T : struct
		{
			VertexCount = vertices.Length;
			if (VertexBufferID == 0)
			{
				SetBuffer(vertices, out VertexBufferID, BufferUsageHint.DynamicDraw);
			}
			else
			{
				UpdateBuffer(vertices, VertexBufferID);
			}
		}

		static void SetBuffer<T>(T[] vertices, out int bufferId, BufferUsageHint usage = BufferUsageHint.StaticDraw) where T : struct
		{
			// Generate Array Buffer Id
			GL.GenBuffers(1, out bufferId);

			// Bind current context to Array Buffer ID
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);

			// Send data to buffer
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * BulletSharp.Math.Vector3.SizeInBytes), vertices, usage);

			// Validate that the buffer is the correct size
			int bufferSize;
			GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
			if (vertices.Length * BulletSharp.Math.Vector3.SizeInBytes != bufferSize)
				throw new ApplicationException("Buffer data not uploaded correctly");

			// Clear the buffer Binding
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		static void UpdateBuffer<T>(T[] vertices, int bufferId) where T : struct
		{
			// Bind current context to Array Buffer ID
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferId);

			// Send data to buffer
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * BulletSharp.Math.Vector3.SizeInBytes), vertices, BufferUsageHint.DynamicDraw);

			// Clear the buffer Binding
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public void SetIndexBuffer(uint[] indices)
		{
			int bufferSize;

			ElementsType = DrawElementsType.UnsignedInt;

			// Generate Array Buffer Id
			GL.GenBuffers(1, out ElementBufferID);

			// Bind current context to Array Buffer ID
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferID);

			// Send data to buffer
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

			// Validate that the buffer is the correct size
			GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
			if (indices.Length * sizeof(uint) != bufferSize)
				throw new ApplicationException("Element array not uploaded correctly");

			// Clear the buffer Binding
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void SetIndexBuffer(ushort[] indices)
		{
			ElementsType = DrawElementsType.UnsignedShort;

			// Generate Array Buffer Id
			GL.GenBuffers(1, out ElementBufferID);

			// Bind current context to Array Buffer ID
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferID);

			// Send data to buffer
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);

			// Validate that the buffer is the correct size
			int bufferSize;
			GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
			if (indices.Length * sizeof(ushort) != bufferSize)
				throw new ApplicationException("Element array not uploaded correctly");

			// Clear the buffer Binding
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void SetIndexBuffer(byte[] indices)
		{
			ElementsType = DrawElementsType.UnsignedByte;

			// Generate Array Buffer Id
			GL.GenBuffers(1, out ElementBufferID);

			// Bind current context to Array Buffer ID
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferID);

			// Send data to buffer
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)indices.Length, indices, BufferUsageHint.StaticDraw);

			// Validate that the buffer is the correct size
			int bufferSize;
			GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
			if (indices.Length != bufferSize)
				throw new ApplicationException("Element array not uploaded correctly");

			// Clear the buffer Binding
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void Dispose()
		{
			if (VertexBufferID != 0)
			{
				GL.DeleteBuffers(1, ref VertexBufferID);
			}
		}
	}
}
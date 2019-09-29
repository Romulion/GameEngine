using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
	public class MeshMorper
	{
		private int VBO;
		private Vertex3D[] verts;
		private int vertStride;

		public MeshMorper(Vertex3D[] vertex, int bufferObject, int vertexSize)
		{
			verts = vertex;

			vertStride = vertexSize;
			VBO = bufferObject;
		}


		public void Morph(Vector4[] morphData, float degree)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			IntPtr point = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);

			foreach (var vertex in morphData)
			{
				int index = (int)vertex.W;
				int offset = index * vertStride;
				Vector3 morphed = vertex.Xyz * degree + verts[index].position;
				verts[index].position = morphed;
				Marshal.WriteInt32(point, offset, BitConverter.ToInt32(BitConverter.GetBytes(morphed.X), 0));
				Marshal.WriteInt32(point, offset + 4, BitConverter.ToInt32(BitConverter.GetBytes(morphed.Y), 0));
				Marshal.WriteInt32(point, offset + 8, BitConverter.ToInt32(BitConverter.GetBytes(morphed.Z), 0));

			}

			GL.UnmapBuffer(BufferTarget.ArrayBuffer);
		}

		public void Morph(Vector3[] morphData, float degree)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			IntPtr point = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);

            int uvOffset = (int)Marshal.OffsetOf(typeof(VertexRigged3D), "uvtex");

            foreach (var vertex in morphData)
			{
				int index = (int)vertex.Z;
				int offset = index * vertStride;
				Vector2 morphed = vertex.Xy * degree + verts[index].uvtex;
				verts[index].uvtex = morphed;
				Marshal.WriteInt32(point, offset + uvOffset, BitConverter.ToInt32(BitConverter.GetBytes(morphed.X), 0));
				Marshal.WriteInt32(point, offset + uvOffset + 4, BitConverter.ToInt32(BitConverter.GetBytes(morphed.Y), 0));
			}

			GL.UnmapBuffer(BufferTarget.ArrayBuffer);
		}
	}
}

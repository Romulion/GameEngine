using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Toys { 

	public struct VertMorphData
    {
		public Vector3 Pos;
		public int ID;
    }
	public struct UVMorphData
    {
		public Vector2 UV;
		public int ID;
	}
	public class MeshMorper
	{
		private int VBO;
		private Vertex3D[] vertices;
		private int verticeStride;
		private List<VertMorphData[]> vert2Update;
		private List<UVMorphData[]> uv2Update;

		public MeshMorper(Vertex3D[] vertex, int bufferObject, int vertexSize)
		{
			vertices = vertex;
			verticeStride = vertexSize;
			VBO = bufferObject;
			//prepare update buffers
			vert2Update = new List<VertMorphData[]>();
			uv2Update = new List<UVMorphData[]>();
		}


		public void Morph(VertMorphData[] morphData, float degree)
		{
			var updated = new VertMorphData[morphData.Length];
			for (int i = 0; i < morphData.Length; i++)
			{
				int index = morphData[i].ID;
				Vector3 morphed = morphData[i].Pos * degree + vertices[index].Position;
				vertices[index].Position = morphed;
				updated[i] = new VertMorphData() { Pos = morphed, ID = index };
			}
			vert2Update.Add(updated);
		}

		public void Morph(UVMorphData[] morphData, float degree)
		{
			var updated = new UVMorphData[morphData.Length];
            for(int i = 0;i < morphData.Length; i++)
			{
				int index = morphData[i].ID;
				Vector2 morphed = morphData[i].UV * degree + vertices[index].UV;
				updated[i] = new UVMorphData() { UV = morphed, ID = index };
				vertices[index].UV = morphed;
			}
			uv2Update.Add(updated);
		}

		internal void PerformMorph()
        {
			if (vert2Update.Count > 0 || uv2Update.Count > 0)
            {
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				IntPtr point = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
				if (vert2Update.Count > 0)
				{
					int posOffset = (int)Marshal.OffsetOf(typeof(VertexRigged3D), "Position");
					foreach (var morphData in vert2Update)
						foreach (var vertex in morphData)
						{
							int index = vertex.ID;
							int offset = index * verticeStride;
							Marshal.WriteInt32(point, posOffset + offset, BitConverter.ToInt32(BitConverter.GetBytes(vertex.Pos.X), 0));
							Marshal.WriteInt32(point, posOffset + offset + 4, BitConverter.ToInt32(BitConverter.GetBytes(vertex.Pos.Y), 0));
							Marshal.WriteInt32(point, posOffset + offset + 8, BitConverter.ToInt32(BitConverter.GetBytes(vertex.Pos.Z), 0));

						}
				}

				if (uv2Update.Count > 0)
                {
					int uvOffset = (int)Marshal.OffsetOf(typeof(VertexRigged3D), "UV");
					foreach (var morphData in uv2Update)
						foreach (var vertex in morphData)
						{
							int index = vertex.ID;
							int offset = index * verticeStride;
							Marshal.WriteInt32(point, uvOffset + offset , BitConverter.ToInt32(BitConverter.GetBytes(vertex.UV.X), 0));
							Marshal.WriteInt32(point, uvOffset + offset + 4, BitConverter.ToInt32(BitConverter.GetBytes(vertex.UV.Y), 0));
						}

				}
				GL.UnmapBuffer(BufferTarget.ArrayBuffer);
				uv2Update.Clear();
				vert2Update.Clear();
			}
        }
	}
}

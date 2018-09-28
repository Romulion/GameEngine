using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
	public class Mesh
	{

		public int[] indexes;
		//public Texture[] textures;
		//public IMaterial[] mats;

		int VAO, VBO, EBO;
		internal int vertexCount;

		int SSB0;

		internal int vSize = 0;
		VertexRigged[] vert;
		public Vertex[] vertTest;

		public Mesh(Vertex[] vertices, int[] indexes)
		{
			this.indexes = indexes;
			SetupMesh(vertices);
		}

		public Mesh(VertexRigged[] vertices, int[] indexes)
		{
			this.indexes = indexes;
			vSize = Marshal.SizeOf(typeof(Vertex));
			//old
			//SetupMeshRigged(vertices);

			//new
			vert = vertices;
			vertTest = new Vertex[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				vertTest[i] = (Vertex)vertices[i];

			SetupMesh(vertTest);

		}



		public void Delete()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(EBO);
			GL.DeleteBuffer(VBO);
		}

		void SetupMesh(Vertex[] vertices)
		{
			Console.WriteLine(vertices[0].position.X);
			vertexCount = vertices.Length;
			int vertSize = Marshal.SizeOf(typeof(Vertex));
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();
			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			//load vertexes to 
			//GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, vertSize * vertexCount, vertices, BufferUsageHint.StreamDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indexes.Length, indexes, BufferUsageHint.StaticDraw);

			Console.WriteLine(Marshal.OffsetOf(typeof(Vertex), "uvtex"));
			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "uvtex"));
			GL.EnableVertexAttribArray(2);

			//unbind
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		void SetupMeshRigged(VertexRigged[] vertices)
		{

			vert = vertices;
			vertexCount = vertices.Length;
			int vertSize = Marshal.SizeOf(typeof(VertexRigged));
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();
			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			//load vertexes to 
			//GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, vertSize * vertexCount, vertices, BufferUsageHint.StreamDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indexes.Length, indexes, BufferUsageHint.StaticDraw);

			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged), "position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged), "normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged), "uvtex"));
			GL.EnableVertexAttribArray(2);
			//bones indexes
			GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, vertSize, Marshal.OffsetOf(typeof(VertexRigged), "boneIndexes"));
			GL.EnableVertexAttribArray(3);
			//bones weigth
			GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged), "weigth"));
			GL.EnableVertexAttribArray(4);
			//unbind
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			//generating shared storage buffer
		}

		void SetupMesh()
		{
			
			//Delete();
			vertexCount = vertTest.Length;
			int vertSize = Marshal.SizeOf(typeof(Vertex));
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();
			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			//load vertexes to buffer
			//GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, vertSize * vertexCount, vertTest, BufferUsageHint.StreamDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indexes.Length, indexes, BufferUsageHint.StaticDraw);

			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex), "uvtex"));
			GL.EnableVertexAttribArray(2);

			//unbind
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		//create shader storage buffer for mesh
		internal void MakeSSBO(int index)
		{
			
			int vertSize = Marshal.SizeOf(typeof(VertexRigged));
			SSB0 = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB0);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, vertSize* vertexCount, vert, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, index, SSB0);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}

		//updating mesh for vertex morph and skinning
		void UpdateMeshRigged()
		{
			vert[100].position.X += 0.01f;
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			int vertSize = Marshal.SizeOf(typeof(VertexRigged));
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, vertSize * vertexCount, vert);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		internal void ApplySkin()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			int size = Marshal.SizeOf(typeof(Vertex)) * vertexCount;

			GL.CopyBufferSubData(BufferTarget.ShaderStorageBuffer, BufferTarget.ArrayBuffer, (IntPtr)0,(IntPtr)0, size);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		internal void BindVAO()
		{
			GL.BindVertexArray(VAO);
		}

		internal void ReleaseVAO()
		{
			GL.BindVertexArray(0);
		}

		internal void BindSSBO()
		{
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB0);
		}
		internal void BindSSBO1()
		{
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, SSB0);
		}

		internal void Draw(int offset, int count)
		{
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset * sizeof(int));
		}

		internal void Draw()
		{
			GL.DrawElements(PrimitiveType.Triangles, vertexCount, DrawElementsType.UnsignedInt, 0);
		}

		/// <summary>
		/// old methods
		/// </summary>
#region old
		/*
		public void Draw()
		{
			//UpdateMesh();
			//draw materials
			GL.BindVertexArray(VAO);
			foreach (var mat in mats)
			{
				if (mat.dontDraw)
					continue;
				mat.ApplyMaterial();
				GL.DrawElements(PrimitiveType.Triangles, mat.count, DrawElementsType.UnsignedInt, mat.offset * sizeof(int));

			}
			GL.BindVertexArray(0);
		}

		//for drawing mesh in one call for shadow map etc
		public void DrawSimple()
		{
			GL.BindVertexArray(VAO);
			GL.DrawElements(PrimitiveType.Triangles, indexes.Length, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);
		}

		public void DrawOutline()
		{
			GL.CullFace(CullFaceMode.Front);
			GL.Enable(EnableCap.CullFace);
			GL.BindVertexArray(VAO);
			foreach (var mat in mats)
			{
				MaterialPMX matp = mat as MaterialPMX;
				if (matp == null || mat.dontDraw || !matp.hasEdge)
					continue;
				matp.ApplyOutline();

				GL.DrawElements(PrimitiveType.Triangles, mat.count, DrawElementsType.UnsignedInt, mat.offset * sizeof(int));

			}
			GL.BindVertexArray(0);
			GL.Disable(EnableCap.CullFace);
		}
		*/
#endregion

	}
}

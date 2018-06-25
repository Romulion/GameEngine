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
		int vertexCount;

		VertexRigged[] vert;
		/*
		public Mesh(Vertex[] vertices, int[] indexes, IMaterial[] mat)
		{
			this.indexes = indexes;
			mats = mat;
			Console.WriteLine(mats.Length);
			SetupMesh(vertices);
		}
*//*
		public Mesh(Vertex[] vertices, int[] indexes, IMaterial mat) : this(vertices, indexes, new IMaterial[] { mat }) { }
*/
		public Mesh(Vertex[] vertices, int[] indexes)
		{
			this.indexes = indexes;
			SetupMesh(vertices);
		}

		public Mesh(VertexRigged[] vertices, int[] indexes)
		{
			this.indexes = indexes;
			SetupMeshRigged(vertices);
		}



		public void Delete()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(EBO);
			GL.DeleteBuffer(VBO);
		}

		void SetupMesh(Vertex[] vertices)
		{

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
			GL.BufferData(BufferTarget.ArrayBuffer, Marshal.SizeOf(typeof(Vertex)) * vertexCount, vertices, BufferUsageHint.StreamDraw);

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


		}

		//updating mesh for vertex morph and skinning
		void UpdateMesh()
		{
			vert[100].position.X += 0.01f;
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			int vertSize = Marshal.SizeOf(typeof(VertexRigged));
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, vertSize * vertexCount, vert);
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


		internal void Draw(int offset, int count)
		{
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset * sizeof(int));
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

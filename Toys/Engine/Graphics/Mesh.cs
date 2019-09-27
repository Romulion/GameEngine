using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
	public class Mesh
	{

		public int[] indexes;

		private MeshMorper morpher = null;

		protected int VAO, VBO, EBO;
		internal int vertexCount;
		int SSB0;

		//for vertex morphing
		//stride 
		internal int vSize;
		//elements array
		public Vertex3D[] vert { get; private set; }
        public VertexRigged3D[] vertRig { get; private set; }

        public Mesh(Vertex3D[] vertices, int[] indexes)
		{
            this.indexes = indexes;
			vSize = Marshal.SizeOf(typeof(Vertex3D));
			vert = vertices;
			SetupMesh(vertices);
		}

		public Mesh(VertexRigged3D[] vertices, int[] indexes)
		{
            vertRig = vertices;

            this.indexes = indexes;
			vSize = Marshal.SizeOf(typeof(VertexRigged3D));
            vert = new Vertex3D[vertices.Length];
			for (int i = 0; i < vert.Length; i++)
				vert[i] = (Vertex3D)vertices[i];

            SetupMesh(vert);
            MakeSSBO(0, vertices);
            //SetupMeshRigged(vertices);
        }


		//clearing resources
		public void Delete()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(EBO);
			GL.DeleteBuffer(VBO);
		}

		/// <summary>
		/// Setups the Vertex array buffer.
		/// For 3d vertex position normals uv
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		void SetupMesh(Vertex3D[] vertices)
		{
			vertexCount = vertices.Length;
			int vertSize = Marshal.SizeOf(typeof(Vertex3D));

			//generate buffers
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();

			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

			//load vertex array to memmory
			GL.BufferData(BufferTarget.ArrayBuffer, vertSize * vertexCount, vertices, BufferUsageHint.StaticDraw);


			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indexes.Length, indexes, BufferUsageHint.StaticDraw);

			//assign binding points
			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex3D), "position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex3D), "normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(Vertex3D), "uvtex"));
			GL.EnableVertexAttribArray(2);

			//unbind
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}


		/// <summary>
		/// Setups the Vertex array buffer.
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		void SetupMeshRigged(VertexRigged3D[] vertices)
		{

			vertexCount = vertices.Length;
			int vertSize = Marshal.SizeOf(typeof(VertexRigged3D));
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
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged3D), "position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged3D), "normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged3D), "uvtex"));
			GL.EnableVertexAttribArray(2);
			//bones indexes
			GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, vertSize, Marshal.OffsetOf(typeof(VertexRigged3D), "boneIndexes"));
			GL.EnableVertexAttribArray(3);
			//bones weigth
			GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, vertSize, Marshal.OffsetOf(typeof(VertexRigged3D), "weigth"));
			GL.EnableVertexAttribArray(4);
			//unbind

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
			//generating shared storage buffer
		}

		//create shader storage buffer for mesh
		void MakeSSBO(int index, VertexRigged3D[] vertRig)
		{
			int vertSize = Marshal.SizeOf(typeof(VertexRigged3D));
			SSB0 = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB0);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, vertSize* vertexCount, vertRig, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, index, SSB0);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, VBO);
        }

		public MeshMorper GetMorpher
		{
			get
			{
				if (morpher == null)
					morpher = new MeshMorper(vert, VBO, vSize);
				
				return morpher; 
			}
		}
		//updating mesh for vertex morph and skinning
		public void UpdateMeshRigged()
		{
			vert[100].position.X += 0.01f;
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			int vertSize = Marshal.SizeOf(typeof(VertexRigged3D));
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, vertSize * vertexCount, vert);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		internal void ApplySkin()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			//int size = Marshal.SizeOf(typeof(Vertex3D)) * vertexCount;

			//GL.CopyBufferSubData(BufferTarget.ShaderStorageBuffer, BufferTarget.ArrayBuffer, (IntPtr)0,(IntPtr)0, size);
            //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}


		internal void BindVAO()
		{
			GL.BindVertexArray(VAO);
		}

		internal void ReleaseVAO()
		{
			GL.BindVertexArray(0);
		}


		//for compute shader morphs
		internal void BindSSBO()
		{
            //GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, SSB0);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, VBO);
        }

		/// <summary>
		/// Draw the part of mesh.
		/// For meshes splitted by material
		/// </summary>
		///
		/// <param name="offset">Offset.</param>
		/// <param name="count">Count.</param>
		internal void Draw(int offset, int count)
		{
			GL.DrawElements(PrimitiveType.Triangles, count, DrawElementsType.UnsignedInt, offset * sizeof(int));
		}

		internal void Draw()
		{
			GL.DrawElements(PrimitiveType.Triangles, indexes.Length, DrawElementsType.UnsignedInt, 0);
		}



	}
}

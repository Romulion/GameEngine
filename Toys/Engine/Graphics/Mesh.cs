using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
	public class Mesh
	{

		public int[] Indices;

		private MeshMorper morpher = null;

		protected int VAO, VBO, EBO;
		internal int VertexCount;
		int SSB0;
        internal bool IsReady { get; private set; }
		//for vertex morphing
		//stride 
		internal int VertexSize;
		//elements array
		public Vertex3D[] Vertices { get; private set; }
        public VertexRigged3D[] vertRig { get; private set; }

        public Mesh(Vertex3D[] vertices, int[] indices)
		{
            Indices = indices;
			VertexSize = Marshal.SizeOf(typeof(Vertex3D));
            VertexCount = vertices.Length;
            Vertices = vertices;
            //check if loading in current context
            if (GLWindow.gLWindow.CheckContext)
            {
                SetupMesh(vertices);
                IsReady = true;
            }
            else
            {
                CoreEngine.ActiveCore.AddTask = () => {
                    SetupMesh(vertices);
                    IsReady = true;
                };
            }
		}

		public Mesh(VertexRigged3D[] vertices, int[] indexes)
		{
            vertRig = vertices;

            Indices = indexes;
			
            VertexCount = vertices.Length;
            Vertices = new Vertex3D[vertices.Length];
			for (int i = 0; i < Vertices.Length; i++)
				Vertices[i] = (Vertex3D)vertices[i];

            VertexSize = Marshal.SizeOf(typeof(VertexRigged3D));
#if !VertexSkin
            if (GLWindow.gLWindow.CheckContext)
            {
                SetupMesh(Vertices);
                MakeSSBO(0, vertices);
                IsReady = true;
            }
            else
            {
                CoreEngine.ActiveCore.AddTask = () => {
                    SetupMesh(Vertices);
                    MakeSSBO(0, vertices);
                    IsReady = true;
                };
            }
            
            
#else
            SetupMeshRigged(vertices);
#endif
        }


		//clearing resources
		internal void Delete()
		{
			GL.DeleteVertexArray(VAO);
			GL.DeleteBuffer(EBO);
			GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(SSB0);
		}

		/// <summary>
		/// Setups the Vertex array buffer.
		/// For 3d vertex position normals uv
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		void SetupMesh(Vertex3D[] vertices)
		{
            var vertexSize = Marshal.SizeOf(typeof(Vertex3D));
            //generate buffers
            VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();

			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

			//load vertex array to memmory
			GL.BufferData(BufferTarget.ArrayBuffer, vertexSize * VertexCount, vertices, BufferUsageHint.StaticDraw);


			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * Indices.Length, Indices, BufferUsageHint.StaticDraw);

			//assign binding points
			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, Marshal.OffsetOf(typeof(Vertex3D), "Position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, vertexSize, Marshal.OffsetOf(typeof(Vertex3D), "Normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, vertexSize, Marshal.OffsetOf(typeof(Vertex3D), "UV"));
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

			
			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			EBO = GL.GenBuffer();
			//bind buffers
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			//load vertexes to 
			//GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, VertexSize * VertexCount, vertices, BufferUsageHint.StreamDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * Indices.Length, Indices, BufferUsageHint.StaticDraw);

			//position
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(typeof(VertexRigged3D), "Position"));
			GL.EnableVertexAttribArray(0);
			//normal
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(typeof(VertexRigged3D), "Normal"));
			GL.EnableVertexAttribArray(1);
			//uv coord
			GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(typeof(VertexRigged3D), "UV"));
			GL.EnableVertexAttribArray(2);
			//bones indexes
			GL.VertexAttribIPointer(3, 4, VertexAttribIntegerType.Int, VertexSize, Marshal.OffsetOf(typeof(VertexRigged3D), "BoneIndices"));
			GL.EnableVertexAttribArray(3);
			//bones weigth
			GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, VertexSize, Marshal.OffsetOf(typeof(VertexRigged3D), "BoneWeigth"));
			GL.EnableVertexAttribArray(4);
			//unbind

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
            //generating shared storage buffer
        }

		//create shader storage buffer for mesh
		void MakeSSBO(int index, VertexRigged3D[] verticesRigged)
		{
			int verticeSize = Marshal.SizeOf(typeof(VertexRigged3D));
			SSB0 = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB0);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, verticeSize* VertexCount, verticesRigged, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, index, SSB0);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, VBO);
        }

		public MeshMorper GetMorpher
		{
			get
			{
				if (morpher == null)
#if VertexSkin
                    morpher = new MeshMorper(vert, VBO, vSize);
#else
                    morpher = new MeshMorper(Vertices, SSB0, VertexSize);
#endif
                return morpher; 
			}
		}
		//updating mesh for vertex morph and skinning
		public void UpdateMeshRigged()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			int vertSize = Marshal.SizeOf(typeof(VertexRigged3D));
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, vertSize * VertexCount, Vertices);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		internal void ApplySkin()
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
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
			GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
		}

        ~Mesh()
        {
            
        }

	}
}

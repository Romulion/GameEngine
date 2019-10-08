using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
    /// <summary>
    /// 11% gpu consumption increase compared to vertex shader skinning
    /// </summary>
	public class ModelSkinning
	{
		Shader _computeShader;
		Mesh _mesh;

        public ModelSkinning(Mesh mesh)
		{
			_mesh = mesh;
			Initialize(mesh.VertexCount,mesh.VertexSize);
		}

		public void Initialize(int vertexCount, int vertexSize)
		{
			var manager = ShaderManager.GetInstance;
			_computeShader =	manager.LoadShader("compute","skin.glsl");
            //CheckData();
        }

        public void Skin()
		{
            _computeShader.ApplyShader();
            _mesh.BindSSBO();
            GL.DispatchCompute(_mesh.VertexCount, 1, 1);
        }

        void CheckData()
        {
            _mesh.BindSSBO();
            _computeShader.ApplyShader();
            GL.DispatchCompute(_mesh.VertexCount, 1, 1);

            _mesh.ApplySkin();
            IntPtr point = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            
            int n = 0, offset = 0, val;
            
            while ( n < _mesh.VertexCount)
            {
                //position
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Position.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Position.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Position.Z != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 4;

                //normals
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Normal.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Normal.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].Normal.Z != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 4;

                //textures
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].UV.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (_mesh.Vertices[n].UV.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 8;
                offset += 4;
                n++;
            }
            
            n = 0;
            offset = 0;
            if (n < _mesh.VertexCount)
            {
                Console.WriteLine("memory mismatch found at {0} total {2} offset {1}",n, offset, _mesh.VertexCount);
                //Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, offset - 4)), 0));
                Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, offset)), 0));
                Console.WriteLine("{0} {1} {2}", _mesh.Vertices[n].Position, _mesh.Vertices[n].Normal, _mesh.Vertices[n].UV);
            }
            else
            {
                Console.WriteLine("memory test ok");
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
        }

	}
}

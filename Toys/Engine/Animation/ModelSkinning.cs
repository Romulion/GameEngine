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
		Shader ComputeShader;
		int SSB;
		Mesh mesh;

		public ModelSkinning(Mesh mesh)
		{
			this.mesh = mesh;
			Initialize(mesh.vertexCount,mesh.vSize);
		}

		public void Initialize(int vCount, int vSize)
		{
			var manager = ShaderManager.GetInstance;
			ComputeShader =	manager.LoadShader("compute","skin.glsl");
            /*
            SSB = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, vCount * vSize, (IntPtr)0, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, SSB);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            */
            //CheckData();
           // Console.WriteLine(GL.GetError());
        }

        public void Skin()
		{
            
			ComputeShader.ApplyShader();
            mesh.BindSSBO();
            GL.DispatchCompute(mesh.vertexCount, 1, 1);
            //Console.WriteLine(GL.GetError());
            //mesh.BindSSBO();
            //GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SSB);
            //GL.DispatchCompute(mesh.vertexCount, 1, 1);
            //GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            //mesh.ApplySkin();
            //*/

            /*
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB);
			IntPtr point = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
			int val = Marshal.ReadInt32(point,0);
			Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(val),0));
			//GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
			*/

        }

        void CheckData()
        {
            //GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SSB);
            mesh.BindSSBO();
            ComputeShader.ApplyShader();
            GL.DispatchCompute(mesh.vertexCount, 1, 1);
            //GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            mesh.ApplySkin();
            IntPtr point = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            //Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, 28)), 0));
            
            int n = 0, offset = 0, val;
            
            while ( n < mesh.vertexCount)
            {
                //position
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].position.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].position.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].position.Z != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 4;

                //normals
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].normal.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].normal.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].normal.Z != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 4;

                //textures
                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].uvtex.X != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;

                offset += 4;
                val = Marshal.ReadInt32(point, offset);
                if (mesh.vert[n].uvtex.Y != BitConverter.ToSingle(BitConverter.GetBytes(val), 0))
                    break;
                //dummy
                offset += 8;
                offset += 4;
                n++;
            }
            
            n = 0;
            offset = 0;
            if (n < mesh.vertexCount)
            {
                Console.WriteLine("memory mismatch found at {0} total {2} offset {1}",n, offset, mesh.vertexCount);
                //Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, offset - 4)), 0));
                Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, offset)), 0));
                Console.WriteLine("{0} {1} {2}", mesh.vert[n].position, mesh.vert[n].normal, mesh.vert[n].uvtex);
            }
            else
            {
                Console.WriteLine("memory test ok");
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
        }

	}
}

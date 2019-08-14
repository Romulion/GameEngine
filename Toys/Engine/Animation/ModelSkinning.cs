using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Toys
{
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
			ComputeShader.ApplyShader();
			mesh.MakeSSBO(0);

            SSB = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer,SSB);
			GL.BufferData(BufferTarget.ShaderStorageBuffer, vCount * vSize, (IntPtr)0, BufferUsageHint.DynamicDraw);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, SSB);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

           // CheckData();
        }

        public void Skin()
		{
            /*
			ComputeShader.ApplyShader();
			//mesh.BindSSBO();
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SSB);
			GL.DispatchCompute(mesh.vertexCount, 1, 1);
			GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            
			mesh.ApplySkin();
            */

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
            

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SSB);
            GL.DispatchCompute(mesh.vertexCount, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, SSB);
            IntPtr point = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);

            Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, 28)), 0));

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

            if (n < mesh.vertexCount)
            {
                Console.WriteLine("memory mismatch found at {0} total {2} offset {1}",n, offset, mesh.vertexCount);
                Console.WriteLine(BitConverter.ToSingle(BitConverter.GetBytes(Marshal.ReadInt32(point, offset)), 0));
            }
            else
            {
                Console.WriteLine("memory test ok");
            }
        }

	}
}

using BulletSharp;
using BulletSharp.Math;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class PhysicsDebugDraw : BufferedDebugDraw
	{
		DynamicsWorld world;
		PrimitiveMeshProcessor pmp;

		public PhysicsDebugDraw(DynamicsWorld physWorld)
		{
			world = physWorld;
			pmp = new PrimitiveMeshProcessor(world);
		}

		public void DrawDebugWorld()
		{
			GL.Clear(ClearBufferMask.DepthBufferBit);
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            pmp.InitInstancedRender();
			pmp.RenderInstanced();

			/*
			GL.UseProgram(0);
			world.DebugDrawWorld();

			if (LineIndex == 0)
				return;


			Vector3[] positionArray = new Vector3[LineIndex];
			int[] colorArray = new int[LineIndex];
			int i;
			for (i = 0; i < LineIndex; i++)
			{
				positionArray[i] = Lines[i].Position;
				colorArray[i] = Lines[i].Color;
			}
			LineIndex = 0;

			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);

			GL.VertexPointer(3, VertexPointerType.Float, 0, positionArray);
			GL.ColorPointer(3, ColorPointerType.UnsignedByte, sizeof(int), colorArray);
			GL.DrawArrays(PrimitiveType.Lines, 0, positionArray.Length);

			GL.DisableClientState(ArrayCap.ColorArray);
			GL.DisableClientState(ArrayCap.VertexArray);
*/
			//GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
		}
	}
}

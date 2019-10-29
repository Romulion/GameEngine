using System;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class PostProcessing
	{
        static int VBO = 0;
        static int VAO = 0;
        int renderBuffer;
        public RenderTexture OutputTexture { get; private set; }
        Camera camera;

        public PostProcessing(Camera camera)
		{
            this.camera = camera;
            InitializeBuffer();
            if (VBO == 0)
            {
                CreateQuad();
            }
		}

        void CreateQuad()
        {
            float[] fboVertices = {
                    -1, -1, 0, 0,
                     1, -1, 1, 0,
                    -1,  1, 0, 1,
                     1,  1, 1, 1,
                };
            VBO = GL.GenBuffer();
            VAO = GL.GenVertexArray();

            int verticeSize = 4 * sizeof(float);
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * fboVertices.Length, fboVertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, verticeSize, (IntPtr)0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, verticeSize, (IntPtr)(verticeSize / 2));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        void InitializeBuffer()
        {
            renderBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderBuffer);
            //create multisampled texture
            OutputTexture = new RenderTexture(camera.Width, camera.Height);
            OutputTexture.AttachToCurrentBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

		public void RenderScreen()
		{
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, renderBuffer);
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        internal void OnResize(int width, int height)
        {
            OutputTexture.ResizeTexture(width,height);
        }
    }
}

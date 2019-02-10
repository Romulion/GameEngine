using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;

namespace Toys
{
    class GraphicsEngine
    {
        int Width = 640, Height = 480;
        int FBO;

        public GraphicsEngine()
        {
            Instalize();
        }



        public void Instalize()
        {
            try
            {

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                //ShaderManager mgr = ShaderManager.GetInstance;
                //mgr.LoadShader("pp");
                //pp = mgr.GetShader("pp");

                //setting aditional buffer
                FBO = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
                Texture texture = Texture.LoadFrameBufer(Width, Height, "postprocess");
                //screen = new Model(texture,pp);
                //Console.WriteLine(GL.GetInteger(GetPName.MaxComputeImageUniforms));
                //allocation custom framebuffer buffers
                int RBO = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
                //Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }


        }

        public Scene Scene { get; set; }


        public void Render()
        {
            // render graphics
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            //	GL.Enable(EnableCap.DepthTest);

            //reducing draw calls
            //if (!trigger)
            //{
            //drawing shadow
            
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Multisample);

            Scene.GetLight.RenderShadow();

            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.CullFace);
            //resize viev to normal size
            GL.Viewport(0, 0, Width, Height);
            //}
            //trigger = !trigger;
            

            //render scene to buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.0f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Scene.Render();

            /*
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			pp.ApplyShader();
			screen.Draw();
*/
            //		pp.ApplyShader();
            //pp.SetUniform(0);
            //			screen.Draw();
            //shdr1.ApplyShader();
            //plane.Draw(shdr1);
        }

        public void Resize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            GL.Viewport(0,0,Width,Height);
            Scene.Resize(Width, Height);

        }
    }
}

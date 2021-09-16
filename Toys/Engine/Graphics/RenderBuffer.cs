using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
    class RenderBuffer
    {
        public RenderTexture RenderTexture { get; private set; }
        public int RenderBufferDraw { get; private set; }

        int RenderBufferOut { get; set; }
        int textureMS, rbo, rboOut, Width, Height;
        readonly int samples;
        bool depthStencil;
        public RenderBuffer(Camera camera,int samplesCount, bool depthStencil)
        {
            Width = camera.Width;
            Height = camera.Height;
            samples = samplesCount;
            this.depthStencil = depthStencil;
            //create render buffer
            InitializeBuffers();
        }

        //downsample multisampled texture
        public void DownSample()
        {
            if (samples > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderBufferDraw);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, RenderBufferOut);
                GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, 
                    ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit, 
                    BlitFramebufferFilter.Nearest);
            }
        }


        public void OnResize(int width, int height)
        {
            Width = width;
            Height = height;
            
            if (samples > 0)
            {
                
                GL.BindTexture(TextureTarget.Texture2DMultisample, textureMS);
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba, Width, Height, true);

                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            }

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboOut);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            RenderTexture.ResizeTexture(Width, Height);
        }

        void InitializeBuffers()
        {
            RenderTexture = new RenderTexture(Width, Height);
            RenderBufferOut = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderBufferOut);
            RenderTexture.AttachToCurrentBuffer();
            rboOut = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboOut);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rboOut);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //create multisampled texture
            if (samples > 0)
            {
                //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                RenderBufferDraw = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderBufferDraw);
                textureMS = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2DMultisample, textureMS);
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba, Width, Height, true);
                GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, textureMS, 0);
                //additional render buffer
                rbo = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                RenderBufferDraw = RenderBufferOut;
            }
        }

        internal void Destroy()
        {
            RenderTexture.Unload();
        }
    }
}

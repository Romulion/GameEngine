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
        public RenderTexture renderTex { get; private set; }
        public int rendBuffMS { get; }

        public int rendBuff { get; }
        int textureMS, rbo, Width, Height;
        int samples = 4;
        public RenderBuffer(Camera cam)
        {
            Width = cam.Width;
            Height = cam.Height;
            //create multisampled buffer
            rendBuffMS = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rendBuffMS);
            //create multisampled texture
            textureMS = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, textureMS);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba, cam.Width, cam.Height, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, textureMS, 0);
            //additional render buffer
            rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, cam.Width, cam.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //create out buffer
            rendBuff = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rendBuff);
            renderTex = new RenderTexture(cam.Width, cam.Height);
            renderTex.AttachToBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //downsample multisampled texture
        public void Update()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, rendBuffMS);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, rendBuff);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }
    }
}

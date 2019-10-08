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
        public int RenderBufferMS { get; }

        public int RenderBufferPost { get; }
        int textureMS, rbo, Width, Height;
        int samples = 4;
        public RenderBuffer(Camera camera)
        {
            Width = camera.Width;
            Height = camera.Height;
            //create multisampled buffer
            RenderBufferMS = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderBufferMS);
            //create multisampled texture
            textureMS = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, textureMS);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba, camera.Width, camera.Height, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, textureMS, 0);
            //additional render buffer
            rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, camera.Width, camera.Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //create out buffer
            RenderBufferPost = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RenderBufferPost);
            RenderTexture = new RenderTexture(camera.Width, camera.Height);
            RenderTexture.AttachToBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        //downsample multisampled texture
        public void Update()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderBufferMS);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, RenderBufferPost);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }
    }
}

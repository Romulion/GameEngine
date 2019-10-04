using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Toys
{
    class RenderTexture : Texture
    {
        public RenderTexture(int Width, int Height)
        {
            base.Width = Width;
            base.Height = Height;
            GL.BindTexture(TextureTarget.Texture2D, texture_id);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            //setting wrapper
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
        }


        internal void AttachToBuffer()
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture_id, 0);
        }

        public Bitmap GetImage()
        {
            Bitmap b = new Bitmap(Width, Height);
            GetImage(b);
            return b;
        }


        public void GetImage(Bitmap bm)
        {
            BindTexture();
            var bits = bm.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GetImage(bits.Scan0);
            bm.UnlockBits(bits);
        }

        public void GetImage(IntPtr imgPoint)
        {
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, imgPoint);
        }

        internal void ResizeTexture(int width, int heigth)
        {
            Width = width;
            Height = heigth;
            GL.BindTexture(TextureTarget.Texture2D, texture_id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          width, heigth, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        }
    }
}

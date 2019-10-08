using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Toys
{
    class RenderTexture : Texture
    {
        public RenderTexture(int width, int height)
        {
            Width = width;
            Height = height;
            GL.BindTexture(TextureTarget.Texture2D, texture_id);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
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
            Bitmap image = new Bitmap(Width, Height);
            GetImage(image);
            return image;
        }


        public void GetImage(Bitmap image)
        {
            BindTexture();
            var imageBits = image.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GetImage(imageBits.Scan0);
            image.UnlockBits(imageBits);
        }

        public void GetImage(IntPtr imagePointer)
        {
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, imagePointer);
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

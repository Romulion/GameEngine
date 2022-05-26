using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Toys
{
    public class RenderTexture : Texture
    {
        Rectangle imageRectanglel;
        public RenderTexture(int width, int height)
        {
            GenerateTextureID();
            Width = width;
            Height = height;
            textureType = TextureTarget.Texture2D;

            BindTexture();
            GL.TexImage2D(textureType, 0, PixelInternalFormat.Rgba,
                          width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            WrapModeU = TextureWrapMode.ClampToEdge;
            WrapModeV = TextureWrapMode.ClampToEdge;
            FillterMode = TextureFillterMode.Bilinear;

            imageRectanglel = new Rectangle(0, 0, Width, Height);
        }


        internal void AttachToCurrentBuffer()
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, textureType, textureID, 0);
        }


        public new void GetImage(Bitmap image)
        {
            BindTexture();
            var imageBits = image.LockBits(imageRectanglel, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GetImage(imageBits.Scan0);
            image.UnlockBits(imageBits);
        }

        public new void GetImage(IntPtr imagePointer)
        {
            GL.GetTexImage(textureType, 0, PixelFormat.Bgra, PixelType.UnsignedByte, imagePointer);
        }

        internal void ResizeTexture(int width, int height)
        {
            Width = width;
            Height = height;
            BindTexture();
            GL.TexImage2D(textureType, 0, PixelInternalFormat.Rgba,
                          width, height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            imageRectanglel = new Rectangle(0, 0, Width, Height);
        }
    }
}

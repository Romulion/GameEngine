using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// Dynamically updating texture from image buffer
    /// </summary>
    public class TextureDynamic: Texture
    {
        Rectangle imageRectanglel;
        internal TextureDynamic(int width, int height)
        {
            GenerateTextureID();
            Width = width;
            Height = height;
            textureType = TextureTarget.Texture2D;

            BindTexture();
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            GL.TexImage2D(textureType, 0, PixelInternalFormat.Rgba,
                          width, height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
            WrapModeU = TextureWrapMode.ClampToEdge;
            WrapModeV = TextureWrapMode.ClampToEdge;
            FillterMode = TextureFillterMode.Bilinear;
            //Start with black color
            var color = new Vector4( 0, 0, 0, 1 );
            GL.ClearTexImage(textureID, 0, PixelFormat.Rgba, PixelType.Float,ref color);

            imageRectanglel = new Rectangle(0, 0, Width, Height);

            //update settings
            BindTexture();
        }


        internal void UpdateTexture(IntPtr ptr)
        {
            BindTexture();
            //loading to video memory
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0,
                          Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, ptr);
        }

    }
}

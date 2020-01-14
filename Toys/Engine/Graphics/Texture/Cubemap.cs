using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;

namespace Toys
{
    public class Cubemap : Texture
    {

        public Cubemap()
        {
            textureID = GL.GenTexture();
            textureType = TextureTarget.TextureCubeMap;
            GL.BindTexture(textureType, textureID);
            //setting wrapper
            GL.TexParameter(textureType, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(textureType, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(textureType, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            //setting interpolation
            GL.TexParameter(textureType, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(textureType, TextureParameterName.TextureMagFilter, (int)All.Linear);
        }

        public Cubemap(string posX, string negX, string posY, string negY, string posZ, string negZ) : this()
        {
            LoadTexture(0, posX);
            LoadTexture(1, negX);
            LoadTexture(2, posY);
            LoadTexture(3, negY);
            LoadTexture(4, posZ);
            LoadTexture(5, negZ);
        }

        void LoadTexture(int side, string location)
        {
            using (Bitmap pic = new Bitmap(ResourcesManager.ReadFromInternalResourceStream("textures.Skybox." + location)))
            {
                System.Drawing.Imaging.BitmapData data =
                  pic.LockBits(new Rectangle(0, 0, pic.Width, pic.Height),
                  System.Drawing.Imaging.ImageLockMode.ReadOnly, pic.PixelFormat);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + side,
                     0, PixelInternalFormat.Rgb, pic.Width, pic.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                pic.UnlockBits(data);
            }
        }
    }
}

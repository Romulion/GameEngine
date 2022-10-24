using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
    public enum TextureWrapMode
    {
        MirrorRepeat = All.MirroredRepeat,
        Repeat = All.Repeat,
        ClampToBorder = All.ClampToBorder,
        ClampToEdge = All.ClampToEdge,
    }

    public enum TextureFillterMode
    {
        Nearest = All.Nearest,
        Bilinear = All.Linear,
        Trilinear = All.LinearMipmapLinear,
    }

    public abstract class Texture : Resource
    {
        protected TextureTarget textureType;
        internal int textureID { get; protected private set; }
        TextureWrapMode wrapU;
        TextureWrapMode wrapV;
        TextureWrapMode wrapW;
        TextureFillterMode filter;

        private bool requireUpdate = false;

        public int Height { get; protected set; }
        public int Width { get; protected set; }

        internal TextureFillterMode FillterMode
        {
            get { return filter; }
            set
            {
                filter = value;
                requireUpdate = true;
            }
        }
        internal TextureWrapMode WrapModeU
        {
            get { return wrapU; }
            set
            {
                wrapU = value;
                requireUpdate = true;
            }
        }

        public TextureWrapMode WrapModeV
        {
            get { return wrapV; }
            set
            {
                wrapV = value;
                requireUpdate = true;
            }
        }

        public TextureWrapMode WrapModeW
        {
            get { return wrapW; } 
            set
            {
                wrapW = value;
                requireUpdate = true;
            }
        }


        public Texture() : base(false) { }

        void UpdateTextureSettings()
        {
            GL.TexParameter(textureType, TextureParameterName.TextureMinFilter, (int)filter);
            GL.TexParameter(textureType, TextureParameterName.TextureMagFilter, (int)filter);

            GL.TexParameter(textureType, TextureParameterName.TextureWrapS, (int)wrapU);
            GL.TexParameter(textureType, TextureParameterName.TextureWrapT, (int)wrapV);
            GL.TexParameter(textureType, TextureParameterName.TextureWrapR, (int)wrapW);
        }

        /// <summary>
        /// Set Wrapping For All Directions
        /// </summary>
        public TextureWrapMode WrapMode
        {
            get { return WrapModeU; }
            set
            {
                WrapModeU = value;
                WrapModeV = value;
                WrapModeW = value;
            }
        }

        public void GetImage(System.Drawing.Bitmap image)
        {
            var imageRectanglel = new System.Drawing.Rectangle(0, 0, Width, Height);
            BindTexture();
            var imageBits = image.LockBits(imageRectanglel, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GetImage(imageBits.Scan0);
            image.UnlockBits(imageBits);
        }

        public void GetImage(IntPtr imagePointer)
        {
            GL.GetTexImage(textureType, 0, PixelFormat.Bgra, PixelType.UnsignedByte, imagePointer);
        }

        protected void GenerateTextureID()
        {
            textureID = GL.GenTexture();
        }
        internal virtual void BindTexture()
        {
            GL.BindTexture(textureType, textureID);
            if (requireUpdate)
            {
                UpdateTextureSettings();
                requireUpdate = false;
            }
        }

        protected override void Unload()
        {
            Logger.Info(String.Format("unload texture {0}",textureID.ToString()));
            GL.DeleteTexture(textureID);
        }
    }
}

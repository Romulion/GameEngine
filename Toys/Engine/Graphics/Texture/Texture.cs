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
        protected Logger logger = new Logger("Texture");
        protected TextureTarget textureType;
        protected int textureID;
        TextureWrapMode wrapU;
        TextureWrapMode wrapV;
        TextureWrapMode wrapW;
        TextureFillterMode filter;

        public int Height { get; protected set; }
        public int Width { get; protected set; }

        public TextureFillterMode FillterMode
        {
            get { return filter; }
            set
            {
                BindTexture();
                filter = value;
                GL.TexParameter(textureType, TextureParameterName.TextureMinFilter, (int)filter);
                GL.TexParameter(textureType, TextureParameterName.TextureMagFilter, (int)filter);
            }
        }
        public TextureWrapMode WrapModeU
        {
            get { return wrapU; }
            set
            {
                BindTexture();
                wrapU = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapS, (int)wrapU);
            }
        }

        public TextureWrapMode WrapModeV
        {
            get { return wrapV; }
            set
            {
                BindTexture();
                wrapV = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapT, (int)wrapV);
            }
        }

        public TextureWrapMode WrapModeW
        {
            get { return wrapW; } 
            set
            {
                BindTexture();
                wrapW = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapR, (int)wrapW);
            }
        }
        /// <summary>
        /// For Inside changes
        /// </summary>
        protected TextureWrapMode wrapModeU
        {
            set
            {
                wrapU = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapS, (int)wrapU);
            }
        }

        protected TextureWrapMode wrapModeV
        {
            set
            {
                wrapV = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapT, (int)wrapV);
            }
        }

        protected TextureWrapMode wrapModeW
        {
            set
            {
                BindTexture();
                wrapW = value;
                GL.TexParameter(textureType, TextureParameterName.TextureWrapR, (int)wrapW);
            }
        }

        /// <summary>
        /// Set Wrapping For All Directions
        /// </summary>
        public TextureWrapMode WrapMode
        {
            get { return WrapModeU; }
            set
            {
                BindTexture();
                wrapModeU = value;
                wrapModeV = value;
                wrapModeW = value;
            }
        }

        public Texture() : base(typeof(Texture2D)) {}

        protected void GenerateTextureID()
        {
            textureID = GL.GenTexture();
        }
        internal virtual void BindTexture()
        {
            GL.BindTexture(textureType, textureID);
        }

        internal override void Unload()
        {
            GL.DeleteTexture(textureID);
        }
    }
}

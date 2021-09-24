using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;
using KtxSharp;

namespace Toys
{
	public enum TextureType
	{
		Diffuse = 0,
		Toon = 1,
		Specular = 2,
		Sphere = 5,
		ShadowMap = 10,
	};

	public class Texture2D : Texture
	{
        TextureType type;
		public string Name { private set; get; }

        //default texture
        static Texture2D defaultTexture;

        public Texture2D(string path, TextureType type) : this(path)
		{
            this.type = type;
        }

		internal Texture2D(string path)
		{
            textureType = TextureTarget.Texture2D;
            GenerateTextureID();

            Bitmap tex1;
            //check texture
            try
			{

                if (path.EndsWith("ktx", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] ktxBytes = File.ReadAllBytes(path);
                    KtxStructure ktxStructure = null;
                    using (MemoryStream ms = new MemoryStream(ktxBytes))
                    {
                        ktxStructure = KtxLoader.LoadInput(ms);
                        LoadTexture(ktxStructure);
                    }
                    return;
                }

                ///cause .NET cant read tga natievly
                ///png , jpg, bmp is ok
                if (path.EndsWith("tga", StringComparison.OrdinalIgnoreCase))
					tex1 = Paloma.TargaImage.LoadTargaImage(path);
                /// .spa| .sph textures is png bmp or jpg textures
                else if (path.EndsWith("spa", StringComparison.OrdinalIgnoreCase) 
                    || path.EndsWith("sph", StringComparison.OrdinalIgnoreCase))
                {
                    Stream strm = File.OpenRead(path);
                    tex1 = new Bitmap(strm);
                }
                //process alpha channel on bmp
                else if (path.EndsWith("bmp", StringComparison.OrdinalIgnoreCase))
                {
                    tex1 = CustomBMPLoader.Load(path);
                }
				else
					tex1 = new Bitmap(path);
				LoadTexture(tex1);
                tex1.Dispose();
            }
			catch (Exception)
			{
                logger.Error("cant load texture " + path,"");
                Texture2D empty = LoadEmpty();
                textureID = empty.textureID;
                Name = empty.Name;
            }
		}

        internal Texture2D(Stream stream, string path)
        {
            textureType = TextureTarget.Texture2D;
            GenerateTextureID();

            Bitmap tex1;
            //check texture
            try
            {

                if (path.EndsWith("ktx", StringComparison.OrdinalIgnoreCase))
                {
                    KtxStructure ktxStructure = null;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        ktxStructure = KtxLoader.LoadInput(ms);
                        LoadTexture(ktxStructure);
                    }
                    return;
                }

                ///cause .NET cant read tga natievly
                ///png , jpg, bmp is ok
                if (path.EndsWith("tga", StringComparison.OrdinalIgnoreCase))
                    tex1 = Paloma.TargaImage.LoadTargaImage(stream);
                /// .spa| .sph textures is png bmp or jpg textures
                else if (path.EndsWith("spa", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith("sph", StringComparison.OrdinalIgnoreCase))
                {
                    tex1 = new Bitmap(stream);
                }
                //process alpha channel on bmp
                else if (path.EndsWith("bmp", StringComparison.OrdinalIgnoreCase))
                {
                    tex1 = CustomBMPLoader.Load(stream);
                }
                else
                    tex1 = new Bitmap(stream);
                LoadTexture(tex1);
                tex1.Dispose();
            }
            catch (Exception)
            {
                logger.Error("cant load texture " + path, "");
                Texture2D empty = LoadEmpty();
                textureID = empty.textureID;
                Name = empty.Name;
            }
        }

        //for framebuffer
        Texture2D(int texture, string name)
		{
            textureType = TextureTarget.Texture2D;
            textureID = texture;
            Name = name;
		}

		//for build in textures
		internal Texture2D(Bitmap tex, TextureType type, string name)
		{
            textureType = TextureTarget.Texture2D;
            textureID = GL.GenTexture();
            this.type = type;
            Name = name;
			LoadTexture(tex);
            ResourcesManager.AddAsset(this, name);
		}

        protected Texture2D()
        {
            textureType = TextureTarget.Texture2D;
            GenerateTextureID();
        }

        void LoadTexture(Bitmap texture)
		{
           
            //for 8bpp formats 
            if (texture.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed || texture.PixelFormat == System.Drawing.Imaging.PixelFormat.Format4bppIndexed || texture.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
			{
                Bitmap clone = new Bitmap(texture.Width, texture.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                
				using (Graphics gr = Graphics.FromImage(clone))
				{
					gr.DrawImage(texture, new Rectangle(0, 0, clone.Width, clone.Height));
				}
				texture = clone;
			}
			BindTexture();
            SetDefault();
            //load to static memory
            System.Drawing.Imaging.BitmapData data =
				texture.LockBits(new Rectangle(0, 0, texture.Width, texture.Height),
					  System.Drawing.Imaging.ImageLockMode.ReadOnly, texture.PixelFormat);

			//recognithing pixel format type
			PixelFormat format;
            if (Image.IsAlphaPixelFormat(texture.PixelFormat))
				format = PixelFormat.Bgra;
			else
				format = PixelFormat.Bgr;

            //BitmapData has 4 bytes row aligment ?
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
            //loading to video memory
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
						  texture.Width, texture.Height, 0, format, PixelType.UnsignedByte, data.Scan0);

            Width = texture.Width;
            Height = texture.Height;
            //clear resources
            texture.UnlockBits(data);
			texture.Dispose();
		}

        void LoadTexture(KtxStructure texture)
        {
            BindTexture();
            SetDefault();
            //setting mip layer count
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, texture.header.numberOfMipmapLevels-1);

            Width = (int)texture.header.pixelWidth;
            Height = (int)texture.header.pixelHeight;

            if (texture.header.glDataType == 0)
            {
                for (int i = 0; i < texture.header.numberOfMipmapLevels; i++)
                {
                    byte[] mipLev = texture.textureData.textureDataOfMipmapLevel[i];
                    GL.CompressedTexImage2D(TextureTarget.Texture2D, i, (InternalFormat)texture.header.glInternalFormat,
                        (int)texture.header.pixelWidth, (int)texture.header.pixelHeight, 0, mipLev.Length, mipLev);
                    //mip level > 0 gives memory error
                    break;
                }
            }
            else
            {
                for (int i = 0; i < texture.header.numberOfMipmapLevels; i++)
                {
                    byte[] mipLev = texture.textureData.textureDataOfMipmapLevel[i];
                    GL.TexImage2D(TextureTarget.Texture2D, i, (PixelInternalFormat)texture.header.glPixelFormat,
                              (int)texture.header.pixelWidth, (int)texture.header.pixelHeight, 0,
                              (PixelFormat)texture.header.glPixelFormat, (PixelType)texture.header.glDataType, mipLev);
                    //mip level > 0 gives memory error
                    break;
                }
            }
        }

		public void ChangeType(TextureType tt)
		{
			type = tt;
		}

		//loading blank texture
		public static Texture2D LoadEmpty()
		{
			if (defaultTexture == null)
			{
				var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
                using (Bitmap pic = new Bitmap(assembly.GetManifestResourceStream("Toys.Resourses.textures.empty.png")))
                {
                    defaultTexture = new Texture2D(pic, TextureType.Toon, "def");
                }
			}

			return defaultTexture;
		}


		//Shadow texture
		public static Texture2D CreateShadowMap(int width, int height)
		{
            var texture = new Texture2D();
            texture.BindTexture();
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 
			              width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			//setting wrapper
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, texture.textureID, 0);
			//for shadow aa
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)All.CompareRefToTexture);
            return texture;
		}

        public static Texture2D CreateCharMap(int width, int heigth)
        {
            var texture = new Texture2D();
            
            GL.BindTexture(TextureTarget.Texture2D, texture.textureID);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
                          width, heigth, 0, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            //setting wrapper
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        internal void AddSubImage(IntPtr bitmap,int x, int y, int w, int h)
        {
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, w, h, PixelFormat.Red, PixelType.UnsignedByte, bitmap);
        }

        private void SetDefault()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.MirroredRepeat);
            //setting interpolation
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
        }
	}
}

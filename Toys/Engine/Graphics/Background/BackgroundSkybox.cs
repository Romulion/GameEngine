using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;

namespace Toys
{
    class BackgroundSkybox : BackgroundBase
    {
        int skyboxVAO, skyboxVBO, cubemapTexture;
        string[] textureSides = {
            "right.jpg",
            "left.jpg",
            "top.jpg",
            "bottom.jpg",
            "front.jpg",
            "back.jpg"
        };

        public BackgroundSkybox()
        {
            CreateTexture();
            CreateBox();
        }
        static BackgroundSkybox()
        {
            string path = "shaders.";
            string vs = ResourcesManager.ReadFromInternalResource(path + "Skybox.vsh");
            string fs = ResourcesManager.ReadFromInternalResource(path + "Skybox.fsh");
            backgroundShader = new ShaderMain(vs, fs);
            backgroundShader.ApplyShader();
            backgroundShader.SetUniform(0, "skybox");
        }

        public override void DrawBackground(Camera camera)
        {
            backgroundShader.ApplyShader();
            Matrix4 look = camera.GetLook;
            look.M41 = look.M42 = look.M43 = 0;
            backgroundShader.SetUniform(look, "view");
            backgroundShader.SetUniform(camera.Projection, "projection");
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemapTexture);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BindVertexArray(skyboxVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);
        }

        void CreateTexture()
        {
            cubemapTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemapTexture);
            //setting wrapper
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)All.ClampToEdge);
            //setting interpolation
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)All.Linear);

            for (int i = 0; i < textureSides.Length; i++)
            {
                using (Bitmap pic = new Bitmap(ResourcesManager.ReadFromInternalResourceStream("textures.Skybox." + textureSides[i])))
                {
                    System.Drawing.Imaging.BitmapData data =
                      pic.LockBits(new Rectangle(0, 0, pic.Width, pic.Height),
                      System.Drawing.Imaging.ImageLockMode.ReadOnly, pic.PixelFormat);

                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                         0, PixelInternalFormat.Rgb, pic.Width, pic.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    pic.UnlockBits(data);
                }
            }
        }

        void CreateBox()
        {
            float[] skyboxVertices = {
                // positions          
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };


            skyboxVAO = GL.GenVertexArray();
            skyboxVBO = GL.GenBuffer();
            GL.BindVertexArray(skyboxVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3,VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);
        }
    }
}

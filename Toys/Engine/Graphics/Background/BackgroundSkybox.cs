using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Mathematics;

namespace Toys
{
    public class BackgroundSkybox : BackgroundBase
    {
        int skyboxVAO, skyboxVBO;
        Cubemap cubemap;
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
            cubemap = new Cubemap(textureSides[0], textureSides[1], textureSides[2], textureSides[3], textureSides[4], textureSides[5]);
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
            cubemap.BindTexture();
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BindVertexArray(skyboxVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);
            
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

        internal override void Unload()
        {
            GL.DeleteVertexArray(skyboxVAO);
            GL.DeleteBuffer(skyboxVBO);
        }
    }
}

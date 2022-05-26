using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using System.IO;
using System.Drawing.Imaging;

namespace Toys
{
    public class DynamicFormStream : ScriptingComponent
    {
        int renderBufferId = 0;
        int offsetX, offsetY;
        int width, height;
        Camera camera;
        Bitmap imageBitmap;
        RenderTexture renderTex;
        RenderBuffer renderBuffer;
        
        BackgroundBase backgroundBackup;

        //post processing
        PostProcessing ppSh;
        Shader shaderPP;

        public bool IsToggle = false;
        public MemoryStream ImageStream { get; private set; }
        void Awake()
        {
            ImageStream = new MemoryStream();
            camera = CoreEngine.GfxEngine.MainCamera;
            width = camera.Width;
            height = camera.Height;

            renderBuffer = new RenderBuffer(camera, 4, false);
            renderBufferId = renderBuffer.RenderBufferDraw;
            renderTex = renderBuffer.RenderTexture;
            imageBitmap = new Bitmap(width, height);

            //postprocessing testing
            ppSh = new PostProcessing(camera);
            ShaderManager shdrm = ShaderManager.GetInstance;
            shdrm.LoadShader("FormPP");
            shaderPP = shdrm.GetShader("FormPP");
            shaderPP.ApplyShader();
            shaderPP.SetUniform(0, "texture_diffuse");
        }


        void Update()
        {
            if (width != camera.Width || height != camera.Height)
            {
                width = camera.Width;
                height = camera.Height;
                renderBuffer.OnResize(width, height);
                ppSh.OnResize(width, height);
                imageBitmap.Dispose();
                imageBitmap = new Bitmap(width, height);
            }

            if (IsToggle)
            {
                IsToggle = false;
                Toggle();
            }
        }

        void PostRender()
        {
            if (camera.RenderBuffer != 0)
            {
                renderBuffer.DownSample();
                shaderPP.ApplyShader();
                GL.ActiveTexture(TextureUnit.Texture0);
                renderTex.BindTexture();
                ppSh.RenderScreen();
                ppSh.OutputTexture.GetImage(imageBitmap);
                ImageStream.SetLength(0);
                imageBitmap.Save(ImageStream,ImageFormat.Png);
            }

        }

        void Toggle()
        {
            if (camera.RenderBuffer != 0)
            {
                ppSh.ClearColorBuffer = false;
                camera.Background = backgroundBackup;
                camera.RenderBuffer = 0;
            }
            else
            {
                ppSh.ClearColorBuffer = true;
                backgroundBackup = camera.Background;
                camera.Background = null;
                camera.RenderBuffer = renderBufferId;
            }
        }

        void OnDestroy()
        {
            imageBitmap.Dispose();
        }
    }
}

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
        bool isLeftButtonMouseDown = false;
        bool keyPressed = false;
        BackgroundBase backgroundBackup;

        //post processing
        PostProcessing ppSh;
        Shader shaderPP;

        public MemoryStream ImageStream { get; private set; }
        void Awake()
        {
            ImageStream = new MemoryStream();
            camera = CoreEngine.gEngine.MainCamera;
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

            KeyboardState keyState = Keyboard.GetState();
            if (keyState[Key.B] && !keyPressed)
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

                keyPressed = true;
            }

            if (!keyState[Key.B] && keyPressed)
                keyPressed = false;
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

        void OnDestroy()
        {
            imageBitmap.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Toys
{
    class DynamicFormScript : ScriptingComponent
    {
        Camera cam;
        RenderTexture renderTex;
        int rendBuff = 0;
        int rbo = 0;
        bool keyPressed = false;
        DynamicForm form;
        int Width, Height;
        Bitmap bm;
        void Awake()
        {
            cam = CoreEngine.gEngine.MainCamera;
            Width = cam.Width;
            Height = cam.Height;
            renderTex = new RenderTexture(cam.Width,cam.Height);
            rendBuff = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer,rendBuff);
            renderTex.AttachToBuffer();

            rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, cam.Width, cam.Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);
            //Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));

            bm = new Bitmap(Width, Height);
            form = new DynamicForm();
            form.Show();
        }


        void Update()
        {
            
            if (Width != cam.Width || Height != cam.Height)
            {
                Width = cam.Width;
                Height = cam.Height;
                renderTex.ResizeTexture(Width, Height);
                form.Width = cam.Width;
                form.Height = cam.Height;
            }
            
            KeyboardState ks = Keyboard.GetState();
            if (ks[Key.B] && !keyPressed)
            {
                if (cam.renderBuffer != 0)
                    cam.renderBuffer = 0;
                else
                    cam.renderBuffer = rendBuff;

                keyPressed = true;
            }

            if (!ks[Key.B] && keyPressed)
                keyPressed = false;
        }

        void PostRender()
        {

            if (cam.renderBuffer != 0)
            {
                
                renderTex.GetImage(bm);
                bm.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
                form.UpdateFormDisplay(bm);
                
                /*
                 var img = renderTex.GetImage();
                // img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);
                form.UpdateFormDisplay(img);
                img.Dispose();
                */
            }
            
            /*
            KeyboardState ks = Keyboard.GetState();
            if (ks[Key.S])
            {
                var img = renderTex.GetImage();
                img.Save("test.png");
            }
            */
        }
    }
}

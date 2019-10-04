using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Forms;

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
        bool isLeftButtonMouseDown = false;
        int offsetX, offsetY;
        RenderBuffer rb;
        void Awake()
        {
            cam = CoreEngine.gEngine.MainCamera;
            Width = cam.Width;
            Height = cam.Height;

            rb = new RenderBuffer(cam);
            rendBuff = rb.rendBuffMS;
            renderTex = rb.renderTex;
            bm = new Bitmap(Width, Height);
            form = new DynamicForm();

            form.LeftMouseDown += LeftMouseDown;
            form.LeftMouseUp += LeftMouseUp;
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
                {
                    cam.renderBuffer = 0;
                    form.Hide();
                }
                else
                {
                    cam.renderBuffer = rendBuff;
                    form.Show();
                }

                keyPressed = true;
            }

            if (!ks[Key.B] && keyPressed)
                keyPressed = false;
        }

        void PostRender()
        {
            if (cam.renderBuffer != 0)
            {
                rb.Update();
                renderTex.GetImage(bm);
                bm.RotateFlip(RotateFlipType.Rotate180FlipX);
                form.UpdateFormDisplay(bm);
            }


            if (isLeftButtonMouseDown)
            {
                form.Left = Cursor.Position.X + offsetX;
                form.Top = Cursor.Position.Y + offsetY;
            }
            
            KeyboardState ks = Keyboard.GetState();
            if (ks[Key.S])
            {
                bm.Save("test.png");
            }
            
        }

        private void LeftMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            offsetX = form.Left - Cursor.Position.X;
            offsetY = form.Top - Cursor.Position.Y;
            isLeftButtonMouseDown = true;
        }

        private void LeftMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isLeftButtonMouseDown = false;
        }
    }
}

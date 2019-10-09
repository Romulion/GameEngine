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
        int renderBufferId = 0;
        int offsetX, offsetY;
        int width, height;
        Camera camera;
        Bitmap imageBitmap;
        DynamicForm form;
        RenderTexture renderTex;
        RenderBuffer renderBuffer;
        bool isLeftButtonMouseDown = false;
        bool keyPressed = false;
        
        
        void Awake()
        {
            camera = CoreEngine.gEngine.MainCamera;
            width = camera.Width;
            height = camera.Height;

            renderBuffer = new RenderBuffer(camera);
            renderBufferId = renderBuffer.RenderBufferMS;
            renderTex = renderBuffer.RenderTexture;
            imageBitmap = new Bitmap(width, height);
            form = new DynamicForm();

            form.LeftMouseDown += LeftMouseDown;
            form.LeftMouseUp += LeftMouseUp;
        }


        void Update()
        {
            
            if (width != camera.Width || height != camera.Height)
            {
                width = camera.Width;
                height = camera.Height;
                renderTex.ResizeTexture(width, height);
                form.Width = camera.Width;
                form.Height = camera.Height;
            }
            
            KeyboardState keyState = Keyboard.GetState();
            if (keyState[Key.B] && !keyPressed)
            {
                if (camera.RenderBuffer != 0)
                {
                    camera.RenderBuffer = 0;
                    form.Hide();
                }
                else
                {
                    camera.RenderBuffer = renderBufferId;
                    form.Show();
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
                renderBuffer.Update();
                renderTex.GetImage(imageBitmap);
                imageBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                form.UpdateFormDisplay(imageBitmap);
            }


            if (isLeftButtonMouseDown)
            {
                form.Left = Cursor.Position.X + offsetX;
                form.Top = Cursor.Position.Y + offsetY;
            }
            
            KeyboardState keyState = Keyboard.GetState();
            if (keyState[Key.S])
            {
                imageBitmap.Save("test.png");
            }
            
        }

        void OnDestroy()
        {
            imageBitmap.Dispose();
            form.Dispose();
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

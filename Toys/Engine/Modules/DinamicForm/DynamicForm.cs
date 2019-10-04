using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Drawing;

namespace Toys
{
    class DynamicForm : Form
    {
        public event MouseEventHandler LeftMouseUp;
        public event MouseEventHandler LeftMouseDown;
        public event MouseEventHandler RightMouseDown;
        public event MouseEventHandler RightMouseUp;
        public event MouseEventHandler MiddleMouseDown;
        public event MouseEventHandler MiddleMouseUp;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BlendOpt
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        //set window style WS_EX_LAYERED
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 524288;
                return createParams;
            }
        }

        private struct Vector2
        {
            public int x;

            public int y;

            public Vector2(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int DeleteDC(IntPtr hdc);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr screenDc, ref Vector2 topPos, ref Vector2 size, IntPtr memDc, ref Vector2 pointSource, int crKey, ref BlendOpt pblend, int dwFlags);
        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int DeleteObject(IntPtr hObject);

        BlendOpt blend;
        public DynamicForm()
        {
            Text = "";
            AllowDrop = false;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            DoubleBuffered = true;
            MouseDown += _MouseDown;
            MouseUp += _MouseUp;


            blend = default(BlendOpt);
            blend.BlendOp = 0;
            blend.BlendFlags = 0;
            blend.AlphaFormat = 1;
            blend.SourceConstantAlpha = 255;
        }

        public void UpdateFormDisplay(Bitmap bm)
        {
            UpdateFormDisplay(bm.GetHbitmap(Color.FromArgb(0)), bm.Width, bm.Height);
        }

        public void UpdateFormDisplay(IntPtr Bitmap, int Width, int Height)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr oldBitmap = IntPtr.Zero;

            oldBitmap = SelectObject(memDc, Bitmap);
            Vector2 size = new Vector2(Width, Height);
            Vector2 pointSource = new Vector2(0, 0);
            Vector2 topPos = new Vector2(Left, Top);
            UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, 2);
            if (Bitmap != IntPtr.Zero)
            {
                SelectObject(memDc, oldBitmap);
                DeleteObject(Bitmap);
            }
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        private void _MouseDown(object sender, MouseEventArgs e)
        {
            MouseEventHandler mouseEventHandler = null;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    mouseEventHandler = LeftMouseDown;
                    break;
                case MouseButtons.Right:
                    mouseEventHandler = RightMouseDown;
                    break;
                case MouseButtons.Middle:
                    mouseEventHandler = MiddleMouseDown;
                    break;
            }
            mouseEventHandler?.Invoke(this, e);
        }

        private void _MouseUp(object sender, MouseEventArgs e)
        {
            MouseEventHandler mouseEventHandler = null;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    mouseEventHandler = LeftMouseUp;
                    break;
                case MouseButtons.Right:
                    mouseEventHandler = RightMouseUp;
                    break;
                case MouseButtons.Middle:
                    mouseEventHandler = MiddleMouseUp;
                    break;
            }
            mouseEventHandler?.Invoke(this, e);
        }
    }
}

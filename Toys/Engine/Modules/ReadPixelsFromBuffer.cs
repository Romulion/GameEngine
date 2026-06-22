using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;


namespace Toys.Engine.Modules
{
    public class ReadPixelsFromBuffer
    {

        public static Bitmap GetScreenImage()
        {
            var camera = CoreEngine.GetCamera;
            var imageBitmap = new Bitmap(camera.Width, camera.Height);
            var imageRectanglel = new Rectangle(0, 0, imageBitmap.Width, imageBitmap.Height);
            var imageBits = imageBitmap.LockBits(imageRectanglel, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, imageBitmap.Width, imageBitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, imageBits.Scan0);
            imageBitmap.UnlockBits(imageBits);

            return imageBitmap;
        }
    }
}

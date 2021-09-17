using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Toys.Debug
{
    public class DebugTexture
    {


        public static void Save2File(Texture texture, string filename)
        {
            Console.WriteLine("{0} {1}", texture.Width, texture.Height);
            var imageBitmap = new Bitmap(texture.Width, texture.Height);
            texture.GetImage(imageBitmap);
            imageBitmap.Save(filename + ".png");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Toys
{
    /// <summary>
    /// Custom bmp image loader
    /// Bitmat cant read alfa channel from bmp files
    /// </summary>
    class CustomBMPLoader
    {
        public static Bitmap Load(string path)
        {
            Stream fs = File.OpenRead(path);
            var file = new BinaryReader(fs);
            file.BaseStream.Position = 0x12;
            int W = file.ReadInt32();
            int H = file.ReadInt32();
            file.BaseStream.Position = 0x1C;
            int pixelSize = file.ReadInt16();
            Bitmap Bmp = null;

            if (pixelSize == 32)
            {
                file.BaseStream.Position = 0x0A;
                var offset = file.ReadInt32();
                file.BaseStream.Position = offset;
                byte[] byteData = file.ReadBytes(4 * W * H);
                GCHandle GCH = GCHandle.Alloc(byteData, GCHandleType.Pinned);
                IntPtr Scan0 = GCH.AddrOfPinnedObject();
                Bmp = new Bitmap(W, H, 4 * W, PixelFormat.Format32bppArgb, Scan0);
                Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                GCH.Free();
            }
            else
            {
                fs.Position = 0;
                Bmp = new Bitmap(fs);
            }
            file.Close();
            fs.Dispose();
            return Bmp;
        }


        public static Bitmap Load(Stream fs)
        {
            var file = new BinaryReader(fs);
            file.BaseStream.Position = 0x12;
            int W = file.ReadInt32();
            int H = file.ReadInt32();
            file.BaseStream.Position = 0x1C;
            int pixelSize = file.ReadInt16();
            Bitmap Bmp = null;

            if (pixelSize == 32)
            {
                file.BaseStream.Position = 0x0A;
                var offset = file.ReadInt32();
                file.BaseStream.Position = offset;
                byte[] byteData = file.ReadBytes(4 * W * H);
                GCHandle GCH = GCHandle.Alloc(byteData, GCHandleType.Pinned);
                IntPtr Scan0 = GCH.AddrOfPinnedObject();
                Bmp = new Bitmap(W, H, 4 * W, PixelFormat.Format32bppArgb, Scan0);
                Bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                GCH.Free();
            }
            else
            {
                fs.Position = 0;
                Bmp = new Bitmap(fs);
            }
            file.Close();
            fs.Dispose();
            return Bmp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

namespace Toys
{
    public enum AnimationFormat
    {
        SMD,
        VMD,
        LMD,
    }

    public class AnimationLoader
    {

        public static Animation Load(Stream stream, string path)
        {
            string extension = path.Substring(path.LastIndexOf('.') + 1);
            extension = extension.ToLower();

            AnimationFormat format = 0;
            switch (extension)
            {
                case "smd":
                    format = AnimationFormat.SMD;
                    break;
                case "vmd":
                    format = AnimationFormat.VMD;
                    break;
                case "lmd":
                    format = AnimationFormat.LMD;
                    break;
                default:
                    throw new Exception("cant recognize file format");
            }
            return Load(stream, path, format);

        }

        public static Animation Load(Stream stream, string filename, AnimationFormat type)
        {
            IAnimationLoader modelLoader = null;
            switch (type)
            {
                case AnimationFormat.SMD:
                    modelLoader = new AnimationSMD(stream, filename);
                    break;
                case AnimationFormat.VMD:
                    modelLoader = new AnimationVMD(stream, filename);
                    break;
                case AnimationFormat.LMD:
                    modelLoader = new AnimationLMD(stream, filename);
                    break;
            }
            return modelLoader.Load();
        }
    }
}

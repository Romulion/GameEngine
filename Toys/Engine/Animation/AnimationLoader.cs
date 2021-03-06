﻿using System;
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

        public static Animation Load(string path)
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
            return Load(path, format);

        }

        public static Animation Load(string filename, AnimationFormat type)
        {
            IAnimationLoader modelLoader = null;
            switch (type)
            {
                case AnimationFormat.SMD:
                    modelLoader = new AnimationSMD(filename);
                    break;
                case AnimationFormat.VMD:
                    modelLoader = new AnimationVMD(filename);
                    break;
                case AnimationFormat.LMD:
                    modelLoader = new AnimationLMD(filename);
                    break;
            }
            return modelLoader.Load();
        }
    }
}

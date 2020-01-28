﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;

namespace Toys
{
    public class RectTransform
    {
        //top right
        public Vector2 anchorMax;
        public Vector2 offsetMax;
        
        //bottom left
        public Vector2 anchorMin;
        public Vector2 offsetMin;

        public RectangleF GlobalRect { get; private set; }

        internal Vector2 Max { get; private set; }
        internal Vector2 Min { get; private set; }
        private Matrix4 transformMat;
        private UIElement baseNode;
        public Matrix4 GlobalTransform
        {
            get
            {
                return transformMat;
            }
        }

        public Matrix4 globalTransform
        {
            get; private set;
        }
        internal RectTransform(UIElement ui) : this()
        {
            baseNode = ui;
        }

        private RectTransform()
        {
            transformMat = Matrix4.Identity;
            anchorMax = new Vector2(0.5f, 0.5f);
            anchorMin = new Vector2(0.5f, 0.5f);
            GlobalRect = new RectangleF(0, 0, 1, 1);
        }

        /*
        public void UpdateGlobalPosition()
        {
            Vector2 screenSize = new Vector2 (1/(float)CoreEngine.gEngine.Width,1/ (float)CoreEngine.gEngine.Height);
            Max = CalcAbsAnchor(anchorMax) + offsetMax * screenSize;
            Min = CalcAbsAnchor(anchorMin) + offsetMin * screenSize;

            GlobalRect = new RectangleF(Min.X, Min.Y, (Max - Min).X, (Max - Min).Y);
            //updating matrix
            transformMat.M11 = GlobalRect.Width;
            transformMat.M22 = GlobalRect.Height;
            transformMat.M41 = GlobalRect.Left;
            transformMat.M42 = GlobalRect.Top;
        }
        */

        public void UpdateGlobalPositionPix()
        {
            Vector2 screenSize = new Vector2(CoreEngine.gEngine.Width,CoreEngine.gEngine.Height);

            Max = CalcAbsAnchor(anchorMax, screenSize) + offsetMax;
            Min = CalcAbsAnchor(anchorMin, screenSize) + offsetMin;

            GlobalRect = new RectangleF(Min.X, Min.Y, (Max - Min).X, (Max - Min).Y);
            
            //updating matrix
            transformMat.M11 = GlobalRect.Width;
            transformMat.M22 = GlobalRect.Height;
            transformMat.M41 = GlobalRect.Left;
            transformMat.M42 = GlobalRect.Top;
            /*
            position = CalculatePosition(text.textCanvas, text.Node.GetTransform);
            position.Z = text.textCanvas.Scale;
            posUniform.SetValue(position);
            vec4 pos = (projection * vec4(vertex.xy * position_scale.z + position_scale.xy, 0.0, 1.0));
            */
        }

        private Vector2 CalcAbsAnchor(Vector2 anchPos, Vector2 screenSize)
        {
            if (baseNode.Parent != null)
            {
                var parent = baseNode.Parent.GetTransform;
                return (parent.Max - parent.Min) * anchPos + parent.Min;
            }
            else
                return anchPos * screenSize;
        }

        public RectTransform Clone()
        {
            var trans = new RectTransform();
            trans.anchorMax = anchorMax;
            trans.offsetMax = offsetMax;
            trans.anchorMin = anchorMin;
            trans.offsetMin = offsetMin;
            trans.UpdateGlobalPositionPix();
            return trans;
        }
    }
}

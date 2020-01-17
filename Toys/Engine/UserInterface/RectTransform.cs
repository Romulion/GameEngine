using System;
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

        internal Vector2 Max;
        internal Vector2 Min;
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
        internal RectTransform(UIElement ui)
        {
            baseNode = ui;
            transformMat = Matrix4.Identity;
            anchorMax = new Vector2(0.5f, 0.5f);
            anchorMin = new Vector2(0.5f, 0.5f);
            GlobalRect = new RectangleF(0, 0, 1, 1);
        }
        public void UpdateGlobalPosition()
        {
            Vector2 screenSize = new Vector2 (1/(float)CoreEngine.gEngine.Width,1/ (float)CoreEngine.gEngine.Height);
            Max = CalcAbsAnchor(anchorMax) + offsetMax * screenSize;
            Min = CalcAbsAnchor(anchorMin) + offsetMin * screenSize;

            GlobalRect = new RectangleF(Min.X, Min.Y, (Max - Min).X, (Max - Min).Y);
            //updating matrix
            transformMat.M11 = GlobalRect.Width;
            transformMat.M22 = GlobalRect.Height;
            //converting 0 - 1 to -1 - 1
            transformMat.M41 = GlobalRect.Left * 2 - (1 - GlobalRect.Width);
            transformMat.M42 = GlobalRect.Top * 2 - (1 - GlobalRect.Height);
        }

        public void UpdateGlobalPositionPix()
        {
            Vector2 screenSize = new Vector2(CoreEngine.gEngine.Width,CoreEngine.gEngine.Height);

            Max = CalcAbsAnchor(anchorMax * screenSize) + offsetMax;
            Min = CalcAbsAnchor(anchorMin * screenSize) + offsetMin;


            GlobalRect = new RectangleF(Min.X, Min.Y, (Max - Min).X, (Max - Min).Y);

            //updating matrix
            transformMat.M11 = GlobalRect.Width;
            transformMat.M22 = GlobalRect.Height;
            transformMat.M41 = GlobalRect.Left;
            transformMat.M42 = GlobalRect.Top;
        }

        private Vector2 CalcAbsAnchor(Vector2 anchPos)
        {
            if (baseNode.Parent != null)
            {
                var parent = baseNode.Parent.GetTransform;
                return (parent.Max - parent.Min) * anchPos + parent.Min;
            }
            else
                return anchPos;
        }
    }
}

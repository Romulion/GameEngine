using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class RectTransform
    {
        public Vector2 anchorMax;
        public Vector2 anchorMin;
        public Vector2 offsetMax;
        public Vector2 offsetMin;

        public Vector2 Size;

        private Matrix4 localT;
        private UIElement baseNode;

        // public Vector3 scale;
        public Matrix4 globalTransform
        {
            get; private set;
        }
        internal RectTransform(UIElement ui)
        {
            baseNode = ui;
        }
        public void UpdateGlobalTransform()
        {
            if (baseNode.Parent != null)
                globalTransform = baseNode.Parent.GetTransform.globalTransform * localT;
            else
                globalTransform = localT;
        }
    }
}

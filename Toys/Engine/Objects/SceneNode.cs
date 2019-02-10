using System;
using System.Collections.Generic;

namespace Toys
{
    class SceneNode
    {
        List<SceneNode> childs;
        public SceneNode parent;
        Transformation transform;
        public Model model = null;

        public SceneNode()
        {
            childs = new List<SceneNode>();
            parent = null;
            transform = new Transformation(this);
        }

        public Transformation GetTransform
        {
            get
            {
                return transform;
            }
        }

        public void SetParent(SceneNode node)
        {
            parent.RemoveChild(this);
            parent = node;

            UpdateTransform();
        }

        public void UpdateTransform()
        {
            transform.UpdateGlobalTransform();
            foreach (var child in childs)
            {
                child.UpdateTransform();
            }
        }

        private void RemoveChild(SceneNode node)
        {
            childs.Remove(node);
        }


    }
}

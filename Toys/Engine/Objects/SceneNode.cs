using System;
using System.Collections.Generic;

namespace Toys
{
    public class SceneNode
    {
        List<SceneNode> childs;
        public SceneNode parent;
        Transformation transform;
		public MeshDrawer model = null;
		public AnimController anim = null;
        public PhysicsManager phys = null;
		public Morph[] morph = null;
		public string Name;
		public bool Active = true;

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
            if (parent != null)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public abstract class VisualComponent : Resource
    {
        internal Mesh Mesh { get; private set; }
        public Material Material { get; set; }
        protected VisualComponent(Type t) : base(t)
        {
            Vertex3D[] verts = new Vertex3D[]
           {
                new Vertex3D(new Vector2(-1,1), new Vector2(0,0)),
                new Vertex3D(new Vector2(-1,-1), new Vector2(0,1)),
                new Vertex3D(new Vector2(1,-1), new Vector2(1,1)),
                new Vertex3D(new Vector2(-1,1), new Vector2(0,0)),
                new Vertex3D(new Vector2(1,-1), new Vector2(1,1)),
                new Vertex3D(new Vector2(1,1), new Vector2(1,0)),
           };
            Mesh = new Mesh(verts, new int[] { 0, 1, 2, 3, 4, 5 });
        }

        public UIElement Node { get; protected set; }

        internal virtual void Draw()
        {
            Mesh.BindVAO();
            Mesh.Draw();
            Mesh.ReleaseVAO();
        }

        internal override void Unload()
        {
            Mesh.Delete();
        }

        internal virtual void AddComponent(UIElement nod)
        {
            if (Node != null)
                throw new Exception("");
            else
            {
                CoreEngine.gEngine.UIEngine.visualComponents.Add(this);
                Node = nod;
            }
        }

        internal virtual void RemoveComponent()
        {
            if (Node != null)
            {
                Node = null;
                CoreEngine.gEngine.UIEngine.visualComponents.Remove(this);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    /// <summary>
    /// Base class for user interface graphics componetns
    /// </summary>
    public abstract class VisualComponent : Resource
    {
        internal Mesh Mesh { get; private set; }
        Logger logger = new Logger("VisualComponent");
        static Mesh defaultMesh;
        static Material defaultMaterial;
        internal static Texture2D defaultTexture { get; private set; }

        /// <summary>
        /// Drawing Material for Visual Component
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Allow assigning multiple components of same type to one object
        /// </summary>
        public bool AllowMultiple { get; protected internal set; }
        protected VisualComponent(Type t) : base(t)
        {

            Mesh = defaultMesh;
            Material = defaultMaterial;
            AllowMultiple = true;
        }

        static VisualComponent()
        {
            //create Quad mesh
            Vertex3D[] verts = new Vertex3D[]
           {
                new Vertex3D(new Vector2(0,1), new Vector2(0,0)),
                new Vertex3D(new Vector2(0,0), new Vector2(0,1)),
                new Vertex3D(new Vector2(1,0), new Vector2(1,1)),
                new Vertex3D(new Vector2(0,1), new Vector2(0,0)),
                new Vertex3D(new Vector2(1,0), new Vector2(1,1)),
                new Vertex3D(new Vector2(1,1), new Vector2(1,0)),
           };
            defaultMesh = new Mesh(verts, new int[] { 0, 1, 2, 3, 4, 5 });

            //load default material
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Texture";

            defaultTexture = Texture2D.LoadEmpty();
        }

        /// <summary>
        /// Base Node of Component
        /// </summary>
        public UIElement Node { get; protected set; }

        internal virtual void Draw()
        {
            Mesh.BindVAO();
            Mesh.Draw();
            Mesh.ReleaseVAO();
        }

        internal override void Unload()
        {
            //Mesh.Delete();
        }

        internal virtual void AddComponent(UIElement nod)
        {
            if (Node != null)
            {
                logger.Error($"Component {Type} already assigned to node {Node.Name}", "");
            }
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

        public abstract VisualComponent Clone();
    }
}

using System;

namespace Toys
{
	public class MeshDrawer : Component
	{
		public Mesh Mesh { get; private set; }
		public Material[] Materials { get; private set; }
		public Morph[] Morphes { get; private set; }


		public bool OutlineDrawing;
		public bool CastShadow;


		public MeshDrawer(Mesh mesh, Material[] materials = null,Morph[] morphes = null) : base (typeof(MeshDrawer))
		{
			Mesh = mesh;
            if (materials != null)
                Materials = materials;
            else
            {
                Materials = new Material[] { new MaterialPMX(new ShaderSettings(), new RenderDirectives()) };
            }
            Morphes = morphes;
		}

		//for single material mesh
		public MeshDrawer(Mesh mesh, Material materials) : this(mesh, new Material[] { materials })
		{
		}

        public virtual void Prepare()
        {

        }
		/*
		 * main drawing
		*/
		public virtual void Draw()
		{
			Mesh.BindVAO();
			foreach (var material in Materials)
			{
				var renderDirectives = material.RenderDirrectives;
				if (!renderDirectives.IsRendered)
					continue;
                if (renderDirectives.NoCull)
                    CoreEngine.gEngine.SetCullMode(FaceCullMode.Disable);
                else
                    CoreEngine.gEngine.SetCullMode(FaceCullMode.Back);
                material.ApplyMaterial();
                if (material.Count != 0)
                    Mesh.Draw(material.Offset, material.Count);
                else
                    Mesh.Draw();
            }
			Mesh.ReleaseVAO();
		}

		//drawing model outline
		public virtual void DrawOutline()
		{
			
			Mesh.BindVAO();

			foreach (var material in Materials)
			{
				var	outline = material.Outline;
				var renderDirectives = material.RenderDirrectives;
				//outline = mat.outline;
				if (!renderDirectives.IsRendered || !renderDirectives.HasEdges)
					continue;
				outline.ApplyOutline();
                if (material.Count != 0)
                    Mesh.Draw(material.Offset, material.Count);
                else
                    Mesh.Draw();
            }
			Mesh.ReleaseVAO();
		}

		//for shadow mapping
		public virtual void DrawSimple()
		{
			Mesh.BindVAO();
			foreach (var mat in Materials)
			{
				
				if (!mat.RenderDirrectives.IsRendered)
					continue;
                //mat.GetShader.ApplyShader();
                if (mat.Count != 0)
                    Mesh.Draw(mat.Offset, mat.Count);
                else
                    Mesh.Draw();

            }

			Mesh.ReleaseVAO();

		}

		internal override void Unload()
		{
			Mesh.Delete();
		}

        internal override void AddComponent(SceneNode nod)
        {
            if (Node != null)
                throw new Exception("");
            else
            {
                CoreEngine.gEngine.meshes.Add(this);
                Node = nod;  
            }
        }

        internal override void RemoveComponent()
        {
            if (Node != null)
            {
                Node = null;
                CoreEngine.gEngine.meshes.Remove(this);
            }

        }
    }
}

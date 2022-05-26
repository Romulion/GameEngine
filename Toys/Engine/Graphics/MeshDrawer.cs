using System;
using System.Collections.Generic;

namespace Toys
{
	public class MeshDrawer : Component
	{
		public Mesh Mesh { get; private set; }
		public Material[] Materials { get; private set; }
		public List<Morph> Morphes { get; private set; }
        public int RenderMask { get; set; }

        public bool OutlineDrawing;
		public bool CastShadow;

		public MeshDrawer(Mesh mesh, Material[] materials = null, List<Morph> morphes = null) : base(false)
		{
			Mesh = mesh;
            if (materials != null)
                Materials = materials;
            else
            {
                Materials = new Material[] { new MaterialPMX(new ShaderSettings(), new RenderDirectives()) };
            }
            Morphes = morphes;
            //set default mask
            RenderMask = 1;
        }

		//for single material mesh
		public MeshDrawer(Mesh mesh, Material materials) : this(mesh, new Material[] { materials })
		{
		}

        internal BoundingBox GetBoundingBox
        {
            get
            {
                return Mesh.BoundingBox;
            }
        }

        internal virtual void Prepare()
        {

        }
		/*
		 * main drawing
		*/
		internal virtual void Draw()
		{
            //skip not loaded models
            if (!Mesh.IsReady)
                return;

			Mesh.BindVAO();
			foreach (var material in Materials)
			{
				var renderDirectives = material.RenderDirrectives;
				if (!renderDirectives.IsRendered)
					continue;
                if (renderDirectives.NoCull)
                    CoreEngine.GfxEngine.SetCullMode(FaceCullMode.Disable);
                else
                    CoreEngine.GfxEngine.SetCullMode(FaceCullMode.Back);
                material.ApplyMaterial();
                if (material.Count != 0)
                    Mesh.Draw(material.Offset, material.Count);
                else
                    Mesh.Draw();
            }
			Mesh.ReleaseVAO();
		}

		//drawing model outline
		internal virtual void DrawOutline()
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
		internal virtual void DrawSimple()
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

        protected override void Unload()
		{
        }

        internal override void AddComponent(SceneNode nod)
        {
            
            if (Node != null)
                throw new Exception("");
            else
            {
                CoreEngine.GfxEngine.meshes.Add(this);
                Node = nod;  
            }
        }

        internal override void RemoveComponent()
        {
            if (Node != null)
            {
                Node = null;
                CoreEngine.GfxEngine.meshes.Remove(this);
            }

        }


        internal override Component Clone()
        {
            var res = new MeshDrawer(Mesh, Materials,Morphes);
            res.OutlineDrawing = OutlineDrawing;
            return res;
        }
    }
}

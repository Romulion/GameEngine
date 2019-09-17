using System;

namespace Toys
{
	public class MeshDrawer : Component
	{
		public Mesh mesh { get; private set; }
		public Material[] mats { get; private set; }
		public Morph[] morph { get; private set; }


		public bool OutlineDrawing;
		public bool CastShadow;


		public MeshDrawer(Mesh mesh, Material[] mats = null,Morph[] mor = null) : base (typeof(MeshDrawer))
		{
			this.mesh = mesh;
            if (mats != null)
                this.mats = mats;
            else
            {
                this.mats = new Material[] { new MaterialPMX(new ShaderSettings(), new RenderDirectives()) };
            }
            morph = mor;
		}

		//for single material mesh
		public MeshDrawer(Mesh mesh, Material mat) : this(mesh, new Material[] { mat })
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
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				var rdirs = mat.rndrDirrectives;
				if (!rdirs.render)
					continue;

				mat.ApplyMaterial();
                if (mat.count != 0)
                    mesh.Draw(mat.offset, mat.count);
                else
                    mesh.Draw();
            }
			mesh.ReleaseVAO();
		}

		//drawing model outline
		public virtual void DrawOutline()
		{
			
			mesh.BindVAO();

			foreach (var mat in mats)
			{
				var	otl = mat.outln;
				var rndr = mat.rndrDirrectives;
				//outline = mat.outline;
				if (!rndr.render || !rndr.hasEdges)
					continue;
				otl.ApplyOutline();
                if (mat.count != 0)
                    mesh.Draw(mat.offset, mat.count);
                else
                    mesh.Draw();
            }
			mesh.ReleaseVAO();
		}

		//for shadow mapping
		public virtual void DrawSimple()
		{
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				
				if (!mat.rndrDirrectives.render)
					continue;
                //mat.GetShader.ApplyShader();
                if (mat.count != 0)
                    mesh.Draw(mat.offset, mat.count);
                else
                    mesh.Draw();

            }

			mesh.ReleaseVAO();

		}

		internal override void Unload()
		{
			mesh.Delete();
		}

        internal override void AddComponent(SceneNode nod)
        {
            if (node != null)
                throw new Exception("");
            else
            {
                CoreEngine.gEngine.meshes.Add(this);
                node = nod;  
            }
        }

        internal override void RemoveComponent()
        {
            if (node != null)
            {
                node = null;
                CoreEngine.gEngine.meshes.Remove(this);
            }

        }
    }
}

using System;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class MeshDrawer
	{
		Mesh mesh;
		public MaterialPMX[] mats;

		public bool OutlineDrawing;
		public bool CastShadow;

		public MeshDrawer(Mesh mesh, MaterialPMX[] mats)
		{
			this.mesh = mesh;
			this.mats = mats;
		}

		public MeshDrawer(Mesh mesh, MaterialPMX mat) : this(mesh, new MaterialPMX[] { mat })
		{
			mat.count = mesh.indexes.Length;
		}

		/*
		 * method for colored drawing
		*/
		public void Draw()
		{
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				if (mat.dontDraw)
					continue;
				mat.ApplyMaterial();
				mesh.Draw(mat.offset, mat.count);
			}
			mesh.ReleaseVAO();
		}

		// drawing model outline
		public void DrawOutline()
		{
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				if (mat.dontDraw || !mat.hasEdge)
					continue;
				mat.ApplyOutline();
				mesh.Draw(mat.offset, mat.count);
			}
			mesh.ReleaseVAO();
		}

		//for shadow mapping
		public void DrawShadow()
		{
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				if (mat.dontDraw)
					continue;
				mesh.Draw(mat.offset, mat.count);
			}
			mesh.ReleaseVAO();
		}
	}
}

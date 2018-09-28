using System;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Collections.Generic;

namespace Toys
{
	public class MeshDrawer
	{
		public Mesh mesh { get; private set; }
		public MaterialPMX[] mats { get; private set; }
		Shader shaderMain;


		public bool OutlineDrawing;
		public bool CastShadow;


		public MeshDrawer(Mesh mesh, MaterialPMX[] mats)
		{

			this.mesh = mesh;
			this.mats = mats;
			shaderMain = mats[0].GetShader;

		}

		//for single material mesh
		public MeshDrawer(Mesh mesh, MaterialPMX mat) : this(mesh, new MaterialPMX[] { mat })
		{
			mat.count = mesh.indexes.Length;
		}

		public Shader GetShader
		{
			get
			{
				return shaderMain;
			}
		}


		/*
		 * main drawing
		*/
		public void Draw()
		{
			shaderMain.ApplyShader();
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

		//drawing model outline
		public void DrawOutline()
		{
			
			mesh.BindVAO();

			foreach (var mat in mats)
			{
				IOutline otl;
				if (mat is IOutline)
					otl = (IOutline)mat;
				else 
					otl = new Outline();
				//outline = mat.outline;
				if (mat.dontDraw || !otl.hasEdge)
					continue;
				otl.ApplyOutline();
				mesh.Draw(mat.offset, mat.count);
			}
			mesh.ReleaseVAO();
		}

		//for shadow mapping
		public void DrawSimple()
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

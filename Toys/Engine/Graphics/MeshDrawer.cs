using System;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Collections.Generic;

namespace Toys
{
	public class MeshDrawer
	{
		public Mesh mesh { get; private set; }
		public IMaterial[] mats { get; private set; }
		Shader shaderMain;


		public bool OutlineDrawing;
		public bool CastShadow;


		public MeshDrawer(Mesh mesh, IMaterial[] mats)
		{

			this.mesh = mesh;
			this.mats = mats;

		}

		//for single material mesh
		public MeshDrawer(Mesh mesh, IMaterial mat) : this(mesh, new IMaterial[] { mat })
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
			mesh.BindVAO();
			foreach (var mat in mats)
			{
				var rdirs = mat.rndrDirrectives;
				if (!rdirs.render)
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
				var	otl = ((Material)mat).outln;
				var rndr = mat.rndrDirrectives;
				//outline = mat.outline;
				if (!rndr.render || !rndr.hasEdges)
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
				
				if (!mat.rndrDirrectives.render)
					continue;
				//mat.GetShader.ApplyShader();
				mesh.Draw(mat.offset, mat.count);
			}

			mesh.ReleaseVAO();

		}

	}
}

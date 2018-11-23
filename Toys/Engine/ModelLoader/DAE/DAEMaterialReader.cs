using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK;

namespace Toys
{

	public class DAEMaterialReader
	{
		const string libmat = "library_materials";
		const string libeff = "library_effects";
		public List<DAEMaterial> mts;


		public DAEMaterialReader(XmlElement xRoot)
		{
			XmlNode libmatsNode = null;
			XmlNode libeffsNode = null;
			foreach (XmlNode xnode in xRoot)
			{
				if (xnode.Name == libmat)
				{
					libmatsNode = xnode;
					break;
				}
			}

			foreach (XmlNode xnode in xRoot)
			{
				if (xnode.Name == libeff)
				{
					libeffsNode = xnode;
					break;
				}
			}

			if (libmatsNode == null || libeffsNode == null)
				throw new Exception();

			var mats = libmatsNode.FindNodes("material");
			mts = new List<DAEMaterial>(mats.Length);
			foreach (var material in mats)
			{
				CompleteMaterial(material, libeffsNode);
			}

		}

		void CompleteMaterial(XmlNode materialNode, XmlNode root)
		{
			var mat = new DAEMaterial();
			mat.id = materialNode.Attributes.GetNamedItem("id").Value;
			mat.Name = materialNode.Attributes.GetNamedItem("name").Value;

			var inst = materialNode.GetNode("instance_effect");

			string effectId = inst.Attributes.GetNamedItem("url").Value.Replace("#", "");
			string diffuseId = "";

			//
			//Console.WriteLine(root.Name);
			//return;

			var effect = root.FindId(effectId);
			var prof = effect.GetNode("profile_COMMON");
			var technique = prof.GetNode("technique");
			var shadingType = technique.FirstChild;
			switch (shadingType.Name)
			{
				case "phong":
					break;
				default :
					throw new Exception(String.Format("unsupported shading type {0}",shadingType.Name));
			}
			foreach (XmlNode setting in shadingType.ChildNodes)
			{
				if (setting.Name == "emission")
					mat.emission = GetColor(setting);
				else if (setting.Name == "ambient")
					mat.ambient = GetColor(setting);
				else if (setting.Name == "specular")
					mat.specular = GetColor(setting);
				else if (setting.Name == "diffuse")
					diffuseId = setting.GetNode("texture").Attributes.GetNamedItem("texture").Value;
			}


			string textureid = "";
			var sampler = prof.FindAttrib(diffuseId, "sid").FirstChild;
			var source = sampler.FindNodes("source");
			if (source.Length == 1)
			{
				textureid = source[0].InnerText;
			}
			else
				throw new Exception();

			var surf = prof.FindAttrib(textureid, "sid").FirstChild;
			mat.textureName = surf.FindNodes("init_from")[0].InnerText;
			if (mat.textureName.IndexOf("_id", 0) > 0)
			{
				mat.textureName = mat.textureName.Remove(mat.textureName.IndexOf("_id", 0));
			}
			mat.textureName += "." + surf.FindNodes("format")[0].InnerText.ToLower();
			mat.txtr = new Texture(mat.textureName, TextureType.diffuse, mat.textureName);
			mts.Add(mat);
		}

		Vector4 GetColor(XmlNode setting)
		{
			string text = setting.FirstChild.InnerText;
			var vector = StringParser.readFloat(text);
			return new Vector4(vector[0],vector[1],vector[2],vector[3]);
		}

		public List<Material> GetMaterials()
		{
			List<Material> mats = new List<Material>(mts.Count);

			for (int i = 0; i < mts.Count; i++)
			{
				//phong
				var shdrst = new ShaderSettings();
				var rddir = new RenderDirectives();
				shdrst.hasSkeleton = true;
				shdrst.recieveShadow = true;
				shdrst.affectedByLight = true;
				rddir.castShadow = true;
				rddir.hasEdges = true;
				shdrst.TextureDiffuse = true;
				shdrst.discardInvisible = true;
				var mat  = new Material(shdrst, rddir);
				mat.Name = mts[i].Name;
				mat.SetTexture(mts[i].txtr, TextureType.diffuse);

				mats.Add(mat);
			}
			return mats;
		}
	}
}

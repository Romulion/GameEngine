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
		public List<DAEMaterial> DAEMaterials;
        internal string dir;

        Dictionary<string, TextureWrapMode> wrapModes = new Dictionary<string, TextureWrapMode>
        {
            ["WRAP"] = TextureWrapMode.Repeat,
            ["MIRROR"] = TextureWrapMode.MirrorRepeat,
            ["CLAMP"] = TextureWrapMode.ClampToEdge,
            ["BORDER"] = TextureWrapMode.ClampToBorder,
            ["NONE"] = TextureWrapMode.ClampToBorder,
        };

		public DAEMaterialReader(XmlElement xRoot, string dir)
		{
            this.dir = dir;
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
			DAEMaterials = new List<DAEMaterial>(mats.Length);
			foreach (var material in mats)
			{
				CompleteMaterial(material, libeffsNode);
			}

		}

		void CompleteMaterial(XmlNode materialNode, XmlNode root)
		{
			var mat = new DAEMaterial();
			mat.ID = materialNode.Attributes.GetNamedItem("id").Value;
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
					mat.Emission = GetColor(setting);
				else if (setting.Name == "ambient")
					mat.Ambient = GetColor(setting);
				else if (setting.Name == "specular")
					mat.Specular = GetColor(setting);
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
			mat.TextureName = surf.FindNodes("init_from")[0].InnerText;
			if (mat.TextureName.IndexOf("_id", 0) > 0)
			{
				mat.TextureName = mat.TextureName.Remove(mat.TextureName.IndexOf("_id", 0));
			}
			mat.TextureName += "." + surf.FindNodes("format")[0].InnerText.ToLower();
			mat.DiffuseTexture = new Texture2D(dir + mat.TextureName, TextureType.Diffuse);

            var wrapS = sampler.FindNodes("wrap_s");
            if (wrapS.Length > 0)
                mat.DiffuseTexture.WrapModeU = wrapModes[wrapS[0].InnerText];
            var wrapT = sampler.FindNodes("wrap_t");
            if (wrapT.Length > 0)
                mat.DiffuseTexture.WrapModeV = wrapModes[wrapT[0].InnerText];
            DAEMaterials.Add(mat);
		}

		Vector4 GetColor(XmlNode setting)
		{
			string text = setting.FirstChild.InnerText;
			var vector = StringParser.readFloat(text);
			return new Vector4(vector[0],vector[1],vector[2],vector[3]);
		}

		public List<Material> GetMaterials()
		{
			List<Material> mats = new List<Material>(DAEMaterials.Count);

			for (int i = 0; i < DAEMaterials.Count; i++)
			{
				//phong
				var shdrst = new ShaderSettings();
				var rddir = new RenderDirectives();
				shdrst.HasSkeleton = true;
				shdrst.RecieveShadow = false;
				shdrst.AffectedByLight = true;
				rddir.CastShadow = false;
				rddir.HasEdges = true;
				shdrst.TextureDiffuse = true;
				shdrst.DiscardInvisible = true;
                shdrst.Ambient = true;
                var mat  = new MaterialPMX(shdrst, rddir);
				mat.Name = DAEMaterials[i].Name;
                mat.UniManager.Set("ambient_color", Vector3.One);
                mat.SetTexture(DAEMaterials[i].DiffuseTexture, TextureType.Diffuse);

				mats.Add(mat);
			}
			return mats;
		}
	}
}

using System;
using System.Xml;
using System.Collections.Generic;

namespace Toys
{
	public static class Extrensions
	{
		public static XmlNode FindId(this XmlNode root, string id)
		{
			XmlNode result = null;
			foreach (XmlNode subnode in root.ChildNodes)
			{
				if (subnode.Attributes == null)
					continue;
				
				XmlNode idAttrib = subnode.Attributes.GetNamedItem("id");

				//if (idAttrib != null)
				//	Console.WriteLine(idAttrib.Value);

				if (idAttrib != null && idAttrib.Value == id)
				{
					return subnode;
				}
				else if (subnode.HasChildNodes)
				{
					XmlNode res = subnode.FindId(id);
					if (res != null)
						result = res;
				}
					
			}
			return result;
		}

		public static XmlNode FindAttrib(this XmlNode root, string val,string attrib)
		{
			XmlNode result = null;
			foreach (XmlNode subnode in root.ChildNodes)
			{
				if (subnode.Attributes == null)
					continue;
				XmlNode idAttrib = subnode.Attributes.GetNamedItem(attrib);

				//if (idAttrib != null)
				//	Console.WriteLine(idAttrib.Value);

				if (idAttrib != null && idAttrib.Value == val)
				{
					return subnode;
				}
				else if (subnode.HasChildNodes)
				{
					XmlNode res = subnode.FindId(val);
					if (res != null)
						result = res;
				}

			}
			return result;
		}

		public static XmlNode[] FindNodes(this XmlNode root, string name)
		{
			List<XmlNode> nodes = new List<XmlNode>();

			if (!root.HasChildNodes)
				return nodes.ToArray();


			foreach (XmlNode node in root.ChildNodes)
			{
				if (node.Name == name)
					nodes.Add(node);
			}

			return nodes.ToArray();
		}

		public static XmlNode GetNode(this XmlNode root, string name)
		{
			if (!root.HasChildNodes)
				return null;

			foreach (XmlNode node in root.ChildNodes)
			{
				if (node.Name == name)
					return node;
			}

			return null;
		}
	}
}

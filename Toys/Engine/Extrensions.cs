﻿using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK;

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


        public static Vector3 ToEulerXYZ(this Quaternion q)
        {
            Vector3 angles;
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);


            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.PI / 2;
                if (sinp < 0)
                    angles.Y = -angles.Y;
            }
            else
                angles.Y = (float)Math.Asin(sinp);

            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}

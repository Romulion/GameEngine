using System;
using System.Xml;
using System.IO;

namespace Toys
{
	public class ReaderDAE : IModelLoader
	{
		string file;
		XmlDocument xDoc;
		string dir;

		public ReaderDAE(string filename)
		{
			file = filename;
			int indx = filename.LastIndexOf('/');
            if (indx >= 0)
                dir = filename.Substring(0, indx) + '/';
            else
                dir = "";

			xDoc = new XmlDocument();
			xDoc.Load(filename);
			LoadLibraries();
		}

		void LoadLibraries()
		{
			XmlElement xRoot = xDoc.DocumentElement;
			// обход всех узлов в корневом элементе
			foreach (XmlNode xnode in xRoot)
			{
				Console.WriteLine(xnode.Name);
			}

		}

		public Model GetModel
		{
			get
			{
				return null;
			}
		}

		public Model GetRiggedModel
		{
			get
			{
				return null;

			}
		}
	}
}

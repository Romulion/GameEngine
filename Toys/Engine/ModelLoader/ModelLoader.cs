using System;
using System.IO;


namespace Toys
{
	public enum ModelFormat
	{
		PMX,
		DAE,
	}

	public class ModelLoader
	{


		public static IModelLoader Load(string filename)
		{
			string extension = filename.Substring( filename.LastIndexOf('.') + 1);
            extension = extension.ToLower();

			ModelFormat format = 0;
			switch (extension)
			{
				case "pmx":
					format = ModelFormat.PMX;
					break;
				case "dae":
					format = ModelFormat.DAE;
					break;
				default :
					throw new Exception("cant recognize file format");
			}

			return Load(filename, format);
		}

		public static IModelLoader Load(string filename, ModelFormat type)
		{
			IModelLoader modelLoader = null;
			switch (type)
			{
				case ModelFormat.PMX :
					modelLoader = new PmxReader(filename);
					break;
				case ModelFormat.DAE :
					modelLoader = new ReaderDAE(filename);
					break;
					
			}
			return modelLoader;
		}
	}
}

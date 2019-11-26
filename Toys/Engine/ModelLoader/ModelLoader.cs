using System;
using System.IO;


namespace Toys
{
	public enum ModelFormat
	{
		PMX,
		DAE,
        LMD,
	}

	public class ModelLoader
	{
        static Logger logger = new Logger("Model Loading");

		public static IModelLoader Load(string filename)
		{
			string fileExtension = filename.Substring( filename.LastIndexOf('.') + 1);
            fileExtension = fileExtension.ToLower();

			ModelFormat format = 0;
			switch (fileExtension)
			{
				case "pmx":
					format = ModelFormat.PMX;
					break;
				case "dae":
					format = ModelFormat.DAE;
					break;
                case "lmd":
                    format = ModelFormat.LMD;
                    break;
                default :
					throw new Exception("cant recognize file format");
			}

			return Load(filename, format);
		}

		public static IModelLoader Load(string filename, ModelFormat type)
		{
			IModelLoader modelLoader = null;

            try
            {
                switch (type)
                {
                    case ModelFormat.PMX:
                        modelLoader = new PmxReader(filename);
                        break;
                    case ModelFormat.DAE:
                        modelLoader = new ReaderDAE(filename);
                        break;
                    case ModelFormat.LMD:
                        modelLoader = new ReaderLMD(filename);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e.Source);
            }

			return modelLoader;
		}
	}
}

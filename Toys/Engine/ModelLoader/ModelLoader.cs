using System;
using System.IO;


namespace Toys
{
	public enum ModelFormat
	{
		PMX = 1,
		DAE = 2,
        LMD = 3,
        PMD = 4,
	}

	public class ModelLoader
	{
        static Logger logger = new Logger("Model Loading");

		public static IModelLoader Load(Stream stream, string filename)
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
                case "pmd":
                    format = ModelFormat.PMD;
                    break;
                default :
                    logger.Error("cant recognize file format", "");
                    break;
			}

			return Load(stream, filename, format);
		}

		public static IModelLoader Load(Stream stream, string filename, ModelFormat type)
		{
			IModelLoader modelLoader = null;

            try
            {
                switch (type)
                {
                    case ModelFormat.PMX:
                        modelLoader = new PmxReader(stream, filename);
                        break;
                    case ModelFormat.DAE:
                        modelLoader = new ReaderDAE(stream, filename);
                        break;
                    case ModelFormat.LMD:
                        modelLoader = new ReaderLMD(stream, filename);
                        break;
                    case ModelFormat.PMD:
                        modelLoader = new PmdReader(stream, filename);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e.TargetSite.Name);
            }
			return modelLoader;
		}
	}
}

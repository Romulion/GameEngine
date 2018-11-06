using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace Toys
{
	public class ReaderSMD
	{
		StreamReader file;
		Bone[] bones;

		public ReaderSMD(string fileName)
		{
			file = new StreamReader(fileName);
			string header = file.ReadLine();

			if (!header.StartsWith("version",StringComparison.OrdinalIgnoreCase))
				throw new Exception("wrong header version");
		}




		void ReadSkeleton()
		{
			if (file.ReadLine() != "nodes")
				throw new Exception("cant find nodes data");

			List<string> rows = new List<string>();
			string row = "";
			do
			{
				row = file.ReadLine();
				rows.Add(row);
			}
			while (row == "end" || file.EndOfStream);

			bones = new Bone[rows.Count];

			if (file.ReadLine() != "skeleton")
				throw new Exception("cant find nodes data");
			
			file.ReadLine();

			string boneData = "";
			do
			{
				boneData = file.ReadLine();
				string[] values = boneData.Split(' ');
				int id = Int32.Parse(values[0]);
				Vector3 pos = new Vector3(Single.Parse(values[1]),Single.Parse(values[2]),Single.Parse(values[3]));
				Vector3 dir = new Vector3(Single.Parse(values[4]), Single.Parse(values[5]), Single.Parse(values[6]));

				string[] val2 = rows[id].Split(' ');
				Vector3 parent = (id >= 0) ? bones[id].Position : Vector3.Zero;

				//bones[id] = new Bone(
			}
			while (boneData == "end" || file.EndOfStream);

		}

	}
}

using System;
using System.IO;
using System.Text;
using OpenTK;

namespace Toys
{
	public class Reader
	{
		BinaryReader stream;
		public byte Encoding;

		public Reader(BinaryReader stream)
		{
			this.stream = stream;
		}
		

		public string readString()
		{
			int length = stream.ReadInt32();

			byte[] buffer = stream.ReadBytes(length);

			if (Encoding == 1)
			{
				return System.Text.Encoding.UTF8.GetString(buffer);
			}
			if (Encoding == 0)
				
				return System.Text.Encoding.Unicode.GetString(buffer);
			
			return "";
		}

        public string readStringB()
        {
            int length = stream.ReadByte();
            byte[] buffer = stream.ReadBytes(length);

            if (Encoding == 1)
            {
                return System.Text.Encoding.UTF8.GetString(buffer);
            }
            if (Encoding == 0)

                return System.Text.Encoding.Unicode.GetString(buffer);

            return "";
        }

        public Vector3 readVector3()
		{
			return new Vector3(stream.ReadSingle(),stream.ReadSingle(),stream.ReadSingle());
		}

		public Vector2 readVector2()
		{
			return new Vector2(stream.ReadSingle(), stream.ReadSingle());
		}

		public Vector4 readVector4()
		{
			return new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
		}

		public int readVal(byte size)
		{
			switch (size)
			{
				case 1:
					return stream.ReadByte();
				case 2:
					return stream.ReadUInt16();
				case 4:
					return stream.ReadInt32();
			}
			return 0;
		}

	}
}

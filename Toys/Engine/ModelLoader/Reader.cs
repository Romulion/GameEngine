using System;
using System.IO;
using System.Text;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;


namespace Toys
{
	public class Reader : BinaryReader
	{
		public byte EncodingType;

		public Reader(Stream stream): base(stream)
		{
		}

        /// <summary>
        /// Read string from stream with int length
        /// </summary>
        /// <returns></returns>
        public string readString()
		{
			int length = ReadInt32();

			byte[] buffer = ReadBytes(length);
            string text = "";

			if (EncodingType == 1)
			{
                text = Encoding.UTF8.GetString(buffer);
			}
			else if (EncodingType == 0)
				text = Encoding.Unicode.GetString(buffer);
            else if (EncodingType == 200)
                text = Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding(932), Encoding.Unicode, buffer));

            return text;
		}

        /// <summary>
        /// Read string from stream with byte length
        /// </summary>
        /// <returns></returns>
        public string readStringB()
        {
            int length = ReadByte();
            byte[] buffer = ReadBytes(length);
            string text = "";

            if (EncodingType == 1)
                text = Encoding.UTF8.GetString(buffer);
            else if (EncodingType == 0)
                text = Encoding.Unicode.GetString(buffer);
            else if (EncodingType == 200)
                text = Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding(932), Encoding.Unicode, buffer));

            return text;
        }

        public string readStringLength(int length,bool deleteZeroByte = true)
        {
            byte[] buffer = ReadBytes(length);
            string text = "";

            if (EncodingType == 1)
                text = Encoding.UTF8.GetString(buffer);
            else if (EncodingType == 0)
                text = Encoding.Unicode.GetString(buffer);
            else if (EncodingType == 200)
                text = Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding(932), Encoding.Unicode, buffer));

            if (deleteZeroByte)
            {
                int removeStart = text.IndexOf('\0');
                if (removeStart >= 0)
                    text = text.Remove(removeStart);
            }

            return text;
        }

        public Vector3 readVector3()
		{
			return new Vector3(ReadSingle(),ReadSingle(),ReadSingle());
		}

		public Vector2 readVector2()
		{
			return new Vector2(ReadSingle(), ReadSingle());
		}

		public Vector4 readVector4()
		{
			return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public int readVal(byte size)
		{
			switch (size)
			{
				case 1:
					return ReadByte();
				case 2:
					return ReadUInt16();
				case 4:
					return ReadInt32();
			}
			return 0;
		}

        public static string jis2utf(byte[] bytes)
        {
            var decoder = CodePagesEncodingProvider.Instance.GetEncoding(932);
            return Encoding.Unicode.GetString(Encoding.Convert(decoder, Encoding.Unicode, bytes));
        }
    }
}

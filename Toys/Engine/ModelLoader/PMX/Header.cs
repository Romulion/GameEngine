using System;
namespace Toys
{
	public class Header
	{
		
		public byte[] Attributes;
		public string Name;
		public string NameEng;
		public string Comment;
		public string CommentEng;


		public byte GetEncoding
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[0];
				else 
					return 0;
			}
		}


		public byte GetAppendixUV
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[1];
				else
					return 0;
			}
		}

		public byte GetVertexIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[2];
				else
					return 0;

			}
		}

		public byte GetTextureIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[3];
				else
					return 0;
			}
		}

		public byte GetMaterialIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[4];
				else
					return 0;
			}
		}

		public byte GetBoneIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[5];
				else
					return 0;
			}
		}

		public byte GetMorphIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[6];
				else
					return 0;
			}
		}

		public byte GetRigidBodyIndexSize
		{
			get
			{
				if (Attributes.Length == 8)
					return Attributes[7];
				else
					return 0;
			}
		}


	}

}

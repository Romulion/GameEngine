using System;
namespace Toys
{
	public class Header
	{
		
		public byte[] attribs;
		public string Name;
		public string NameEng;
		public string Comment;
		public string CommentEng;


		public byte GetEncoding
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[0];
				else 
					return 0;
			}
		}


		public byte GetAppendixUV
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[1];
				else
					return 0;
			}
		}

		public byte GetVertexIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[2];
				else
					return 0;

			}
		}

		public byte GetTextureIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[3];
				else
					return 0;
			}		}

		public byte GetMaterialIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[4];
				else
					return 0;
			}		}

		public byte GetBoneIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[5];
				else
					return 0;
			}		}

		public byte GetMorphIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[6];
				else
					return 0;
			}		}

		public byte GetRigidBodyIndexSize
		{
			get
			{
				if (attribs.Length == 8)
					return attribs[7];
				else
					return 0;
			}		}


	}

}

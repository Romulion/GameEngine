using System;
using OpenTK;

namespace Toys
{
	internal struct Character
	{
		internal Texture texture;
		internal Vector2 Size;
		internal Vector2 Bearing;
		internal int Advance;

		internal Character(Texture tex, Vector2 s, Vector2 b, int a)
		{
			texture = tex;
			Size = s;
			Bearing = b;
			Advance = a;
		}
	}
}

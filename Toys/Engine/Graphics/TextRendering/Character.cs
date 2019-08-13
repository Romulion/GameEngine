using System;
using OpenTK;

namespace Toys
{
	internal struct Character
	{
		internal Vector2 Size;
		internal Vector2 Bearing;
		internal int Advance;
        internal Vector2 position;

		internal Character(Vector2 p, Vector2 s, Vector2 b, int a)
		{
			Size = s;
			Bearing = b;
			Advance = a;
            position = p;
		}
	}
}

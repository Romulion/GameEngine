using System;
using OpenTK.Mathematics;

namespace Toys
{
	internal struct Character
	{
		internal Vector2 Size;
		internal Vector2 Bearing;
		internal int Advance;
        internal Vector2 Position;

		internal Character(Vector2 position, Vector2 size, Vector2 bearing, int advance)
		{
			Size = size;
			Bearing = bearing;
			Advance = advance;
            Position = position;
		}
	}
}

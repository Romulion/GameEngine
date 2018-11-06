using System;
using System.Collections;

namespace Toys
{
	public class MaterialFlags
	{
		public bool noCull;
		public bool groundShadow;
		public bool receiveShadow;
		public bool vertexColour;
		public bool pointDrawing;
		public bool lineDrawing;
		public bool drawShadow;
		public bool hasEdge;

		public MaterialFlags(byte flgs)
		{
			BitArray flags = new BitArray(new byte[] { flgs });
			noCull = flags[0];
			groundShadow = flags[1];
			drawShadow = flags[2];
			receiveShadow = flags[3];
			hasEdge = flags[4];
			vertexColour = flags[5];
			pointDrawing = flags[6];
			lineDrawing = flags[7];

		}
	}
}

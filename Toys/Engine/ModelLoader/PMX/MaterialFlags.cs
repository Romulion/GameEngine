using System;
using System.Collections;

namespace Toys
{
	public class MaterialFlags
	{
		public bool NoCull;
		public bool CastGroundShadow;
		public bool ReceiveShadow;
		public bool VertexColor;
		public bool PointDrawing;
		public bool LineDrawing;
		public bool CastShadow;
		public bool HasEdge;

		public MaterialFlags(byte flgs)
		{
			BitArray flags = new BitArray(new byte[] { flgs });
			NoCull = flags[0];
			CastGroundShadow = flags[1];
			CastShadow = flags[2];
			ReceiveShadow = flags[3];
			HasEdge = flags[4];
			VertexColor = flags[5];
			PointDrawing = flags[6];
			LineDrawing = flags[7];

		}
	}
}

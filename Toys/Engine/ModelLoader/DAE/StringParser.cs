using System;
namespace Toys
{
	public static class StringParser
	{
		public static int[] readIntArray(string array)
		{
			string[] arr = array.Split(' ');
			int[] res = new int[arr.Length];
			for (int n = 0; n < arr.Length; n++)
			{
				int num = 0;
				Int32.TryParse(arr[n], out num);
				res[n] = num;
			}
			return res;
		}

		public static float[] readFloatArray(string array, float multiplier = 1f)
		{
			string[] arr = array.Split(' ');
			float[] res = new float[arr.Length];

			for (int n = 0; n < arr.Length; n++)
			{
				float num = 0;
				Single.TryParse(arr[n], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"), out num);
				res[n] = num * multiplier;
			}
			return res;
		}
	}
}

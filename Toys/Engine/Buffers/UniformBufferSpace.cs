using OpenTK;
namespace Toys
{
	/// <summary>
	/// buffer for storaging model space matrices
	/// </summary>
	public class UniformBufferSpace : UniformBuffer
	{
		const int matSize = 64;
		const int size = 5 * matSize;
		public UniformBufferSpace(int bindingPoint) : base (size, "space", bindingPoint)
		{
		}

		public void SetModelSpace(Matrix4 mat)
		{
			SetMatrix(mat, 0);
		}

		public void SetPVMSpace(Matrix4 mat)
		{
			SetMatrix(mat, matSize);
		}

		public void SetNormalSpace(Matrix4 mat)
		{
			SetMatrix(mat, 2 * matSize);
		}

		public void SetLightSpace(Matrix4 mat)
		{
			SetMatrix(mat, 3 * matSize);
		}
        /*
		public void SetPvSpace(Matrix4 mat)
		{
			SetMatrix(mat, 4 * matSize);
		}
        */
        public void SetVievModelSpace(Matrix4 mat)
        {
            SetMatrix(mat, 4 * matSize);
        }
    }
}

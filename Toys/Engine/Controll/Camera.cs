using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
namespace Toys
{
	public enum ProjectionType
	{ 
		Perspective = 1,
		Orthographic = 2
	}

    public enum DisplayClearFlag
    {
       Nothing,
       Color,
       Depth,
    }


	public class Camera : Component
	{
        
        public bool Active { get; set;}
        public bool Main { get; internal set; }

        public BackgroundBase Background;

        public int renderBuffer = 0;
        //camera space
        Vector3 cameraTarget = new Vector3(0f, 1f, 0f);
		Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
		Matrix4 look;

        public Color4 ClearColor = new Color4(0.0f, 0.1f, 0.1f, 0f);
        ProjectionType projType = ProjectionType.Perspective;
        public ProjectionType projectionType {
            get { return projType; }
            set { projType = value; }
        }

        float NearPlane = 0.1f;
        float FarPlane = 10.0f;
        public Matrix4 projection { get; private set; }
        public int Width;
        public int Height;
        public int OrthSize;


        public Camera() : base(typeof(Camera))
		{
            projType = ProjectionType.Perspective;
            Main = true;
        }

        internal void CalcProjection()
        {
            if (projType == ProjectionType.Perspective)
                projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Height, NearPlane, FarPlane);
            else if (projType == ProjectionType.Orthographic)
                projection = Matrix4.CreateOrthographic(Width, Height, NearPlane, FarPlane);
        }

        #region Transforms
        public Vector3 GetPos
		{
			get { return node.GetTransform.Position; }
		}
		public Matrix4 GetLook
		{
			get { return look; }
		}
		public Vector3 GetUp
		{
			get { return cameraUp; }
		}
        public Vector3 Target
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }

		internal void CalcLook()
		{
            look = Matrix4.LookAt(node.GetTransform.Position, cameraTarget, cameraUp);
        }
        #endregion
        internal override void AddComponent(SceneNode nod)
        {
            node = nod;
            CoreEngine.gEngine.MainCamera = this;
        }

        internal override void RemoveComponent()
        {
            node = null;
            CoreEngine.gEngine.MainCamera = null;
        }
        internal override void Unload()
        {
        }
    }


}

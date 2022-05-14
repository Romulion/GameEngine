using System;
using OpenTK.Mathematics;
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

    [Serializable]
    public class Camera : Component
	{
        [SaveScene]
        public bool Active { get; set;}
        public bool Main { get; internal set; }

        [SaveScene]
        public int RenderMask { get; set; }

        public BackgroundBase Background;

        internal int RenderBuffer = 0;
        //camera space
        Vector3 cameraTarget = new Vector3(0f, 0f, -1f);
		Matrix4 look;

        public Color4 ClearColor = new Color4(0.0f, 0.1f, 0.1f, 0f);
        ProjectionType projType = ProjectionType.Perspective;
        [SaveScene]
        public ProjectionType projectionType {
            get { return projType; }
            set { projType = value; }
        }
        [SaveScene]
        public float NearPlane { get; set;}
        [SaveScene]
        public float FarPlane { get; set;}
        [SaveScene]
        public float FOV { get; set; }
        
        internal Matrix4 Projection { get; private set; }
        [SaveScene]
        public int Width { get; internal set; }
        [SaveScene]
        public int Height { get; internal set; }

        int cameraHeigth = 2;
        public Camera()
		{
            NearPlane = 0.1f;
            FarPlane = 100.0f;
#if VR
            FOV = 86;
#else
            FOV = 60;
#endif
            projType = ProjectionType.Perspective;
            Main = true;
            RenderMask = 1;
        }

        internal void CalcProjection()
        {
            if (projType == ProjectionType.Perspective)
                Projection = Matrix4.CreatePerspectiveFieldOfView(MathF.PI * (FOV / 180f), Width / (float)Height, NearPlane, FarPlane);
            else if (projType == ProjectionType.Orthographic)
                Projection = Matrix4.CreateOrthographic(cameraHeigth * Width / (float)Height, cameraHeigth, NearPlane, FarPlane);
        }

#region Transforms
        public Vector3 GetPos
		{
			get { return Node.GetTransform.GlobalTransform.ExtractTranslation(); }
		}
        internal Matrix4 GetLook
		{
			get { return look; }
		}
		internal Vector3 GetUp
		{
			get { return Node.GetTransform.Up; }
		}
        public Vector3 Target
        {
            get { return cameraTarget; }
            set { cameraTarget = value; }
        }

		internal void CalcLook()
		{
            look = Matrix4.LookAt(Node.GetTransform.GlobalPosition, (new Vector4(cameraTarget, 1) * Node.GetTransform.GlobalTransform).Xyz, GetUp);
        }
        #endregion
        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.gEngine.MainCamera = this;
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.gEngine.MainCamera = null;
        }
        protected override void Unload()
        {
        }
    }


}

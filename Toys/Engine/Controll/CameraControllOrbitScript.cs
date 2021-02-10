using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Toys
{
    public class CameraControllOrbitScript : ScriptingComponent
    {
        GameWindow game;
        Transform transform;
        Camera camera;
        //mouse controll variables;
        float lastX, lastY;
        int phi = 90, theta = 90, thetaMax = 170, thetaMin = 70;
        public int angleStep = 4, angleThresold = 2;
        public float speed = 0.01f;

        private Vector2 wheel = Vector2.Zero;
        private bool mousePressed;

        //camera space
        const float R = 3.5f;
        float r;
        Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 cameraOffset = new Vector3(0.0f, 0.5f, 0.0f);

        void Awake()
        {
            transform = Node.GetTransform;
            r = 3.5f;
            game = GLWindow.gLWindow;
            camera = (Camera)Node.GetComponent<Camera>();
            camera.Target = Vector3.UnitY;
            camera.Target -= new Vector3(0,0.25f,0);
            transform.Position = camera.Target + CalcPos(r, phi, theta);
        }

        void MouseOrbit()
        {
            var mouseState = GLWindow.gLWindow.MouseState;
            if (game.IsFocused && !CoreEngine.gEngine.UIEngine.Busy && mousePressed && mouseState.IsButtonDown(MouseButton.Left))
            {

                if (mouseState.X - lastX > angleThresold)
                {
                    phi += angleStep;
                }
                else if (mouseState.X - lastX < -angleThresold)
                {
                    phi -= angleStep;
                }

                if (mouseState.Y - lastY > angleThresold && theta < thetaMax)
                {
                    theta += angleStep;
                }
                else if (mouseState.Y - lastY < -angleThresold && theta > thetaMin)
                {
                    theta -= angleStep;
                }

                transform.Position = camera.Target + CalcPos(r, phi, theta);

                lastY = mouseState.Y;
                lastX = mouseState.X;
            }
            else if (!mousePressed)
            {
                mousePressed = true;
                lastX = mouseState.X;
                lastY = mouseState.Y;
            }
            else
                mousePressed = false;

            if (game.IsFocused)
            {
                var mDelta = mouseState.Scroll - wheel;
                /*
                if (mDelta == 0)
                    return;
                if (mDelta > 0)
                    r -= speed * 8f;
                else if (mDelta < 0)
                    r += speed * 8f;
                wheel = mouseState.Wheel;
                */
            }
            transform.Position = camera.Target + CalcPos(r, phi, theta);
        }

        //setuping camera movement
        void Update()
        {
            MouseOrbit();
            var keyState = GLWindow.gLWindow.KeyboardState;
            // camera strafe
            if (game.IsFocused)
            {
                if (keyState.IsKeyDown(Keys.PageUp))
                {
                    camera.Target += speed * cameraUp;
                    transform.Position += speed * cameraUp;
                }

                if (keyState.IsKeyDown(Keys.PageDown))
                {
                    camera.Target -= speed * cameraUp;
                    transform.Position -= speed * cameraUp;
                }

                Vector3 front = camera.Target - transform.Position;
                Vector3 left = Vector3.Cross(front,cameraUp);
                //movment
                
                if (keyState.IsKeyDown(Keys.W))
                {
                    camera.Target += speed * front;
                    transform.Position += speed * front;
                }

                if (keyState.IsKeyDown(Keys.A))
                {
                    camera.Target -= speed * left;
                    transform.Position -= speed * left;
                }

                if (keyState.IsKeyDown(Keys.S))
                {
                    camera.Target -= speed * front;
                    transform.Position -= speed * front;
                }

                if (keyState.IsKeyDown(Keys.D))
                {
                    camera.Target += speed * left;
                    transform.Position += speed * left;
                }
                
                if (keyState.IsKeyDown(Keys.R))
                {
                    transform.Position = new Vector3(0f, 1f, 0f);

                    phi = 90;
                    theta = 90;
                    r = R;
                    camera.Target = Vector3.UnitY;
                    transform.Position += CalcPos(r, phi, theta);
                }
            }
        }

        //calculate decarth coords from spherical
        Vector3 CalcPos(float r, int Iphi, int Itheta)
        {
            float x, y, z;
            float phi = radians(Iphi);
            float theta = radians(Itheta);

            x = r * (float)Math.Sin(theta) * (float)Math.Cos(phi);
            z = r * (float)Math.Sin(theta) * (float)Math.Sin(phi);
            y = r * -(float)Math.Cos(theta);
            return new Vector3(x, y, z);
        }

        float radians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }
    }
}

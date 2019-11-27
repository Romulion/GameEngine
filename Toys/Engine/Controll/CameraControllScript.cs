using System;
using OpenTK;
using OpenTK.Input;

namespace Toys
{
    public class CameraControllScript : ScriptingComponent
    {
        GameWindow game;
        Transformation transform;
        Camera camera;
        //mouse controll variables;
        float lastX, lastY;
        int phi = 90, theta = 90, thetaMax = 170, thetaMin = 70;
        int angleStep = 4, angleThresold = 2;
        bool mousePressed;
        float speed = 0.01f;

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
            Control();
        }

        public void Control()
        {
            game.UpdateFrame += Movement;

            game.MouseWheel += (sender, e) =>
            {
                if (e.Delta > 0)
                    r -= speed * 5f;
                else if (e.Delta < 0)
                    r += speed * 5f;

                transform.Position = camera.Target + CalcPos(r, phi, theta);
            };
        }

        void MouseOrbit()
        {
            var mouseState = Mouse.GetState();
            if (game.Focused && mousePressed && mouseState.IsButtonDown(MouseButton.Left))
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

        }

        //setuping camera movement
        void Movement(object sender, FrameEventArgs e)
        {
            MouseOrbit();
            var keyState = Keyboard.GetState();
            // camera strafe
            if (keyState[Key.Up])
            {
                camera.Target += speed * cameraUp;
                transform.Position += speed * cameraUp;
            }

            if (keyState[Key.Down])
            {
                camera.Target -= speed * cameraUp;
                transform.Position -= speed * cameraUp;
            }

            if (keyState[Key.R])
            {
                transform.Position = new Vector3(0f, 1f, 0f);

                phi = 90;
                theta = 90;
                r = R;
                camera.Target = Vector3.UnitY;
                transform.Position += CalcPos(r, phi, theta);
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

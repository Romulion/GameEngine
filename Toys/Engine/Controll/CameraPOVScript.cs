using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;


namespace Toys
{
    public class CameraPOVScript : ScriptingComponent
    {
        Camera camera;
        Vector4 cameraDir;
        GLWindow game;
        bool mousePressed = false;
        int phi = 90;
        int theta = 90;
        int thetaMax = 170;
        int thetaMin = 10;
        int lastX = 0;
        int lastY = 0;
        public int angleStep = 4, angleThresold = 2;

        void Awake()
        {
            camera = (Camera)Node.GetComponent<Camera>();
            cameraDir = new Vector4(0, 0, 1, 1);
            game = GLWindow.gLWindow;
        }
        void Update()
        {
            MouseControll();
        }
        void MouseControll()
        {
            var mouseState = Mouse.GetState();
            if (game.Focused && !CoreEngine.gEngine.UIEngine.Busy && mousePressed && mouseState.IsButtonDown(MouseButton.Left))
            {

                if (mouseState.X - lastX > angleThresold)
                {
                    phi += angleStep;
                }
                else if (mouseState.X - lastX < -angleThresold)
                {
                    phi -= angleStep;
                }

                if (phi > 360) phi -= 360;
                if (phi < 0) phi += 360;

                if (mouseState.Y - lastY > angleThresold && theta <= thetaMax)
                {
                    theta -= angleStep;
                }
                else if (mouseState.Y - lastY < -angleThresold && theta >= thetaMin)
                {
                    theta += angleStep;
                }

                if (theta > thetaMax)
                    theta = thetaMax;
                else if (theta < thetaMin)
                    theta = thetaMin;

                Node.GetTransform.RotationQuaternion = CalculateRotation(1, phi, theta);
                Node.GetTransform.UpdateGlobalTransform();

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

            camera.Target = (cameraDir * Node.GetTransform.GlobalTransform).Xyz;
        }


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

        Quaternion CalculateRotation(float r, int Iphi, int Itheta)
        {
            var look = cameraDir.Xyz;
            var dir = CalcPos(r, Iphi, Itheta);
            dir.Normalize();
            Vector3 axis = Vector3.Cross(look, dir);
            return Quaternion.FromAxisAngle(axis, (float)Math.Acos(Vector3.Dot(dir, look)));
        }

        float radians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }
    }
}

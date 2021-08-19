using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace Toys
{
    public class CameraPOVScript : ScriptingComponent
    {
        Vector4 cameraDir;
        GLWindow game;
        bool mousePressed = false;
        int phi = -90;
        int theta = 90;
        int thetaMax = 170;
        int thetaMin = 10;
        float lastX = 0;
        float lastY = 0;
        public int angleStep = 4, angleThresold = 2;

        void Awake()
        {
            cameraDir = new Vector4(0, 0, -1, 1);
            game = GLWindow.gLWindow;
        }
        void Update()
        {
            MouseControll();
        }
        void MouseControll()
        {
            var mouseState = GLWindow.gLWindow.MouseState;
            if (game.IsFocused && !CoreEngine.gEngine.UIEngine.Busy && mousePressed && mouseState.IsButtonDown(MouseButton.Button1))
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
                    theta += angleStep;
                }
                else if (mouseState.Y - lastY < -angleThresold && theta >= thetaMin)
                {
                    theta -= angleStep;
                }
                if (theta > thetaMax)
                    theta = thetaMax;
                else if (theta < thetaMin)
                    theta = thetaMin;
                Node.Parent.GetTransform.RotationQuaternion = CalculateRotation(1, phi, 90);
                
                Node.GetTransform.RotationQuaternion = CalculateRotation(1, -90, theta);
                Node.Parent.GetTransform.UpdateGlobalTransform();
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

        public void RecalculateAngles()
        {
            var dir = new Vector4(0, 0, -1, 1);
            var newdir = (dir * Node.GetTransform.GlobalTransform).Xyz - Node.GetTransform.GlobalTransform.ExtractTranslation();
            phi = (int)(MathF.Atan2(newdir.Z, newdir.X) * 180 / MathF.PI);
            theta = (int)(MathF.Acos(newdir.Y / newdir.Xzy.Length) * 180 / MathF.PI);
        }

        Vector3 CalcPos(float r, int Iphi, int Itheta)
        {
            float x, y, z;
            float phi = radians(Iphi);
            float theta = radians(Itheta);

            x = r * MathF.Sin(theta) * MathF.Cos(phi);
            z = r * MathF.Sin(theta) * MathF.Sin(phi);
            y = r * -MathF.Cos(theta);
            return new Vector3(x, y, z);
        }

        Quaternion CalculateRotation(float r, int Iphi, int Itheta)
        {
            var look = cameraDir.Xyz;
            var dir = CalcPos(r, Iphi, Itheta);
            dir.Normalize();
            Vector3 axis = Vector3.Cross(look, dir);
            return Quaternion.FromAxisAngle(axis, MathF.Acos(Vector3.Dot(dir, look)));
        }

        float radians(float degrees)
        {
            return MathF.PI * degrees / 180.0f;
        }
    }
}

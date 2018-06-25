using System;
using OpenTK;
using OpenTK.Input;

namespace Toys
{
	public class Camera
	{
		MouseDevice mouse;
		GameWindow game;

		//mouse controll variables;
		float lastX, lastY;
		int Phi = 90, Theta = 90, ThetaMax = 170, ThetaMin = 70;
		int angleStep = 4, angleThresold = 2;
		bool mousePressed;
		float speed = 0.01f;

		//camera space
		const float R = -3.5f;
		float r;
		Vector3 cameraWorld = new Vector3(0f, 1f, 0f);
		Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 cameraPos;

		Matrix4 look;

		public Camera()
		{
		}

		public Camera(GameWindow game)
		{
			mouse = game.Mouse;
			this.game = game;
			game.UpdateFrame += Movement;

            mouse.WheelChanged += (sender, e) =>
            {
                if (e.Delta > 0)
                    r += speed * 5f;
                else if (e.Delta < 0)
                    r -= speed * 5f;

                cameraPos = CalcPos(r, Phi, Theta);
                CalcLook();
            };

            r = R;
			cameraPos = CalcPos(r, Phi, Theta);

			CalcLook();
		}
		/*
		//setuping mouse controll
		void MouseControll(object sender, FrameEventArgs e)
		{
			if (!mouse.GetState().IsButtonDown(MouseButton.Left))
				return;
			//track mouse
			float xoffset = (mouse.X - lastX) * sensitivity;
			float yoffset = (lastY - mouse.Y) * sensitivity;

			yaw   += xoffset;
			pitch += yoffset;  
			if(pitch > 89.0f)
				pitch =  89.0f;
			if(pitch< -89.0f)
				pitch = -89.0f;
			//set direction
			Vector3 front = Vector3.One;
			front.X = (float)Math.Cos(radians(pitch)) * (float)Math.Cos(radians(yaw));
			front.Y = (float)Math.Sin(radians(pitch));
			front.Z = (float)Math.Cos(radians(pitch)) * (float)Math.Sin(radians(yaw));
			cameraFront = front.Normalized();
		}
		*/

		void MouseOrbit()
		{
			if (mousePressed && mouse.GetState().IsButtonDown(MouseButton.Left))
			{

				if (mouse.X - lastX > angleThresold)
				{
					Phi += angleStep;
				}
				else if (mouse.X - lastX < -angleThresold)
				{
					Phi -= angleStep;
				}

				if (mouse.Y - lastY > angleThresold && Theta < ThetaMax)
				{
					Theta += angleStep;
				}
				else if (mouse.Y - lastY < -angleThresold && Theta > ThetaMin)
				{
					Theta -= angleStep;
				}

				cameraPos = CalcPos(r, Phi, Theta);

				CalcLook();

				lastY = mouse.Y;
				lastX = mouse.X;
			}
			else if (!mousePressed)
			{
				mousePressed = true;
				lastX = mouse.X;
				lastY = mouse.Y;
			}
			else
				mousePressed = false;

		}

		//setuping camera movement
		public void Movement(object sender, FrameEventArgs e) 
		{
            MouseOrbit();
            // camera strafe
            if (game.Keyboard[Key.Up])
			{
				cameraWorld += speed * cameraUp;
                CalcLook();
			}
			                   
			if (game.Keyboard[Key.Down])
            {
				cameraWorld -= speed * cameraUp;
                CalcLook();
            }
				
			/*
			if (game.Keyboard[Key.Left])
            {
				cameraWorld -= speed* Vector3.Cross(-cameraPos, cameraUp).Normalized();
                CalcLook();
			}
				                   
			if (game.Keyboard[Key.Right])
            {
				cameraWorld += speed* Vector3.Cross(-cameraPos, cameraUp).Normalized();
                CalcLook();
            }
            */

			if (game.Keyboard[Key.R])
            {
				cameraWorld = new Vector3(0f, 1f, 0f);

				Phi = 90;
				Theta = 90;
				r = R;
				cameraPos = CalcPos(r, Phi, Theta);
                CalcLook();
            }
            
		}

		public Vector3 GetPos
		{
			get { return cameraPos; }
		}
		public Matrix4 GetLook
		{
			get { return look; }
		}

		public Vector3 GetUp
		{
			get { return cameraUp; }
		}

		//calculate decarth coords from spherical
		Vector3 CalcPos(float r, int Iphi, int Itheta)
		{
			float x, y, z;
			float phi = radians(Iphi);
			float theta = radians(Itheta);

			x = r * (float)Math.Sin(theta) * (float)Math.Cos(phi);
			z = r * (float)Math.Sin(theta) * (float)Math.Sin(phi);
			y = r * (float)Math.Cos(theta);
			return new Vector3(x, y, z);
		}

		void CalcLook()
		{
			look = Matrix4.LookAt(cameraWorld + cameraPos, cameraWorld, cameraUp);
		}

		float radians(float degrees)
		{
			return (float)Math.PI * degrees / 180.0f;
		}

	}


}

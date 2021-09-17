using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Toys.VR
{
    class VRRenderer : MainRenderer
    {
        readonly RenderBuffer leftBuffer;
        readonly RenderBuffer rightBuffer;
        readonly VRSystem vrSystem;

        internal VRRenderer(): base()
        {
            vrSystem = CoreEngine.vrSystem;
            CoreEngine.gEngine.Resize((int)vrSystem.width, (int)vrSystem.height);
            leftBuffer = new RenderBuffer(CoreEngine.GetCamera, 4, true);
            rightBuffer = new RenderBuffer(CoreEngine.GetCamera, 4, true);

            //leftBuffer.OnResize((int)vrSystem.width, (int)vrSystem.height);
            //rightBuffer.OnResize((int)vrSystem.width, (int)vrSystem.height);

            //Console.WriteLine("{0} {1}", CoreEngine.GetCamera.Width, CoreEngine.GetCamera.Height);
        }


        public override void Render(MeshDrawer[] meshes, Camera camera)
        {
            GL.Viewport(0, 0, camera.Width, camera.Height);
            var reye = CalcRightEyePosition(camera);
            camera.Node.GetTransform.Position -= reye;
            //render scene to left eye
            camera.CalcLook();
            

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, leftBuffer.RenderBufferDraw);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //render background first due to model transperancy
            if (camera.Background != null)
                camera.Background.DrawBackground(camera);

            RenderScene(meshes, camera);
            leftBuffer.DownSample();
            vrSystem.SetLeftEye(leftBuffer.RenderTexture.textureID);


            //render scene to right eye
            camera.Node.GetTransform.Position += 2 * reye;
            camera.CalcLook();

            //render scene to left eye
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rightBuffer.RenderBufferDraw);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //render background first due to model transperancy
            if (camera.Background != null)
                camera.Background.DrawBackground(camera);

            RenderScene(meshes, camera);
            rightBuffer.DownSample();
            vrSystem.SetRightEye(rightBuffer.RenderTexture.textureID);

            //return camera back
            camera.Node.GetTransform.Position -= reye;
        }

        Vector3 CalcRightEyePosition(Camera camera)
        {
            var eyeDisplaysment = vrSystem.IPD * 0.0005f;
            var right = camera.Node.GetTransform.Right;
            return right * eyeDisplaysment;
        }
        internal override void Destroy()
        {
            /*
            imageBitmapL.UnlockBits(bitmapDataL);
            imageBitmapR.UnlockBits(bitmapDataR);
            imageBitmapL.Dispose();
            imageBitmapR.Dispose();
            */
            leftBuffer.Destroy();
            rightBuffer.Destroy();

            base.Destroy();
        }
    }
}

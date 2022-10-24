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

        Matrix4[] projections = new Matrix4[2];

        internal VRRenderer(): base()
        {
            vrSystem = CoreEngine.VRSystem;
            CoreEngine.GfxEngine.Resize((int)vrSystem.width, (int)vrSystem.height);
            leftBuffer = new RenderBuffer(CoreEngine.GetCamera, 4, true);
            rightBuffer = new RenderBuffer(CoreEngine.GetCamera, 4, true);

            projections[0] = VRControllerSystem.ConvertMatrix(vrSystem.VRContext.GetEyeToHeadTransform(Valve.VR.EVREye.Eye_Left));
            projections[1] = VRControllerSystem.ConvertMatrix(vrSystem.VRContext.GetEyeToHeadTransform(Valve.VR.EVREye.Eye_Right));
        }


        internal override void Render(MeshDrawer[] meshes, Camera camera, UIEngine ui)
        {

            GL.Viewport(0, 0, camera.Width, camera.Height);
            var reye = CalcRightEyePosition(camera);
            camera.Node.GetTransform.Position -= reye;
            //render scene to left eye
            camera.CalcLook();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, leftBuffer.RenderBufferDraw);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //set up left eye projection
            camera.Projection = vrSystem.controllerSystem.HMD.Prejections[0];

            //render background first due to model transperancy
            if (camera.Background != null)
                camera.Background.DrawBackground(camera);

            RenderScene(meshes, camera);
            ui.DrawWorldUI(camera);

            leftBuffer.DownSample();
            vrSystem.SetLeftEye(leftBuffer.RenderTexture.textureID);


            //render scene to right eye
            camera.Node.GetTransform.Position += 2 * reye;
            camera.CalcLook();

            //render scene to left eye
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rightBuffer.RenderBufferDraw);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            //set up right eye projection
            camera.Projection = vrSystem.controllerSystem.HMD.Prejections[1];

            //render background first due to model transperancy
            if (camera.Background != null)
                camera.Background.DrawBackground(camera);

            RenderScene(meshes, camera);
            ui.DrawWorldUI(camera);

            rightBuffer.DownSample();
            vrSystem.SetRightEye(rightBuffer.RenderTexture.textureID);

            //return camera back
            camera.Node.GetTransform.Position -= reye;            
            camera.CalcLook();
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

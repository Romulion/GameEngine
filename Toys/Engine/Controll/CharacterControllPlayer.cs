using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using BulletSharp;
using Toys;

namespace Toys
{
    public class CharacterControllPlayer : CharacterControllBase
    {
        GLWindow game;
        CollisionWorld world;
        Camera camera;
        //RayResultCallback rayCalback;
        Action<SceneNode> lookCallback;
        CollisionObject activeObject;
        new void Awake()
        {
            base.Awake();
            game = GLWindow.gLWindow;
            world = CoreEngine.pEngine.World;
            camera = CoreEngine.GetCamera;
            
            /*
            rayCalback = new KinematicClosestNotMeRayResultCallback(_ghostObject);
            rayCalback.CollisionFilterMask = (int)CollisionFilleters.Look;
            rayCalback.CollisionFilterGroup = (int)CollisionFilleters.Look;
            */
        }

        new void Update()
        {
            if (CoreEngine.iSystem.CurrentContext != InputContext.Main)
                return;

            var rayCalback = new KinematicClosestNotMeRayResultCallback(_ghostObject);
            rayCalback.CollisionFilterMask = (int)CollisionFilleters.Look;
            rayCalback.CollisionFilterGroup = (int)CollisionFilleters.Look;
            //var test1 = Vector3.Cross(new Vector3(-6.84475f, 1.5930859f, 11.550375f) -  camera.GetPos, (camera.GetLook * 10 * -Vector4.UnitZ).Xyz);
            //Console.WriteLine("{0} {1} {2}", camera.GetPos.Convert(), (camera.GetLook * 10 * -Vector4.UnitZ).Xyz.Convert(), test1.Length / (camera.GetLook * 10 * -Vector4.UnitZ).Xyz.Length);
            world.RayTest(camera.GetPos.Convert(),(camera.GetLook * 10 * new Vector4(0,0,-1,1)).Xyz.Convert(), rayCalback);
            if (rayCalback.HasHit)
            {
               // Console.WriteLine("Hit ON {0}",rayCalback.HitNormalWorld);
                if (rayCalback.CollisionObject.UserIndex == 1)
                {
                    if (activeObject != rayCalback.CollisionObject)
                    {
                        activeObject = rayCalback.CollisionObject;
                        lookCallback?.Invoke(null);
                        lookCallback = (Action<SceneNode>)activeObject.UserObject;
                    }
                    lookCallback(camera.Node);
                }
                
            }

            /*
            else if (activeObject != null)
            {
                //reset look
                lookCallback(Vector3.Zero);
                lookCallback = null;
                activeObject = null;
            }
            */

            var keyState = GLWindow.gLWindow.KeyboardState;
            if (game.IsFocused && keyState.IsKeyDown(Keys.Up)){
                Walk(-Vector3.UnitZ);
            }
            else if (game.IsFocused && keyState.IsKeyDown(Keys.Down))
            {
                Walk(Vector3.UnitZ);
            }
            else if (game.IsFocused && keyState.IsKeyDown(Keys.Left))
            {
                Walk(-Vector3.UnitX);
            }
            else if (game.IsFocused && keyState.IsKeyDown(Keys.Right))
            {
                Walk(Vector3.UnitX);
            }
            else if (game.IsFocused && keyState.IsKeyDown(Keys.Space))
            {
                Jump();
            }
            else
            {
                Stop();
            }
            
            base.Update();
            
        }

        public void Walk(Vector3 dir)
        {
            Walk(dir, CoreEngine.frameTimer.FrameTime);
        }

        public void LoadPos(Vector3 pos)
        {
            base.Teleport(pos);
        }
    }
}


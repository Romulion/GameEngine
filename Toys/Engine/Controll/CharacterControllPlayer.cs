using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using BulletSharp;

namespace Toys
{
    public class CharacterControllPlayer : CharacterControllBase
    {
        GLWindow game;
        CollisionWorld world;
        Camera camera;
        //RayResultCallback rayCalback;
        Action<Vector3> lookCallback;
        CollisionObject activeObject;
        new void Awake()
        {
            base.Awake();
            game = GLWindow.gLWindow;
            world = CoreEngine.pEngine.World;
            camera = Node.GetComponent<Camera>();
            /*
            rayCalback = new KinematicClosestNotMeRayResultCallback(_ghostObject);
            rayCalback.CollisionFilterMask = (int)CollisionFilleters.Look;
            rayCalback.CollisionFilterGroup = (int)CollisionFilleters.Look;
            */
        }

        new void Update()
        {
            var rayCalback = new KinematicClosestNotMeRayResultCallback(_ghostObject);
            rayCalback.CollisionFilterMask = (int)CollisionFilleters.Look;
            rayCalback.CollisionFilterGroup = (int)CollisionFilleters.Look;
            world.RayTest(camera.GetPos.Convert(),(camera.GetLook * -Vector4.UnitZ * 10).Xyz.Convert(), rayCalback);
            if (rayCalback.HasHit)
            {
                
                if (rayCalback.CollisionObject.UserIndex == 1)
                {
                    if (activeObject != rayCalback.CollisionObject)
                    {
                        activeObject = rayCalback.CollisionObject;
                        lookCallback?.Invoke(Vector3.Zero);
                        lookCallback = (Action<Vector3>)activeObject.UserObject;
                    }
                    lookCallback(camera.GetPos);
                }
                
            }
            else if (activeObject != null)
            {
                //reset look
                lookCallback(Vector3.Zero);
                lookCallback = null;
                activeObject = null;
            }


            var keyState = GLWindow.gLWindow.KeyboardState;
            if (game.IsFocused && keyState.IsKeyDown(Keys.Up)){
                Walk(CoreEngine.ActiveCore.elapsed);
            }
            else if (game.IsFocused && keyState.IsKeyDown(Keys.Down))
            {
                Walk(-CoreEngine.ActiveCore.elapsed);
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

        public void LoadPos(Vector3 pos)
        {
            base.Teleport(pos);
        }
    }
}


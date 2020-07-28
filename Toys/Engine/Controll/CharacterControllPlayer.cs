using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
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

            

            KeyboardState kb = Keyboard.GetState();
            if (game.Focused && kb.IsKeyDown(Key.Up)){
                Walk(CoreEngine.ActiveCore.elapsed);
            }
            else if (GLWindow.gLWindow.Focused && kb.IsKeyDown(Key.Down))
            {
                Walk(-CoreEngine.ActiveCore.elapsed);
            }
            else if (GLWindow.gLWindow.Focused && kb.IsKeyDown(Key.Space))
            {
                Jump();
            }
            else
            {
                Stop();
            }
            
            base.Update();
            
        }

    }
}


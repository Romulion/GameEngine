using System;
using Toys;
using OpenTK;
using BulletSharp;

namespace ModelViewer
{
    
    class NpcAI : ScriptingComponent
    {
        public bool Busy { get; private set; }
        float headSpeed = (float)(1 * Math.PI / 180);
        BoneTransform head;
        int i = 0;
        BoxShape headBox;
        RigidBody headBody;
        bool looked;
        Vector3 lookTo;
        Vector3 axis;
        float angle;
        void Start()
        {
            var rigged = Node.GetComponent<Animator>();
            if (rigged)
                head = rigged.BoneController.GetBone("頭");

            //create head ray collision object
            headBox = new BoxShape(0.3f);
            var rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix4.Identity.Convert()), headBox, BulletSharp.Math.Vector3.Zero);
            headBody = new RigidBody(rbInfo);
            CoreEngine.pEngine.World.AddRigidBody(headBody, (int)CollisionFilleters.Look, (int)CollisionFilleters.Look);
            //headBody.CollisionFlags |= CollisionFlags.KinematicObject;
            //ManifoldPoint.ContactAdded = 
            headBody.UserObject = new Action<Vector3>(triggerSwitch);
            headBody.UserIndex = 1;
        }

        void Update()
        {
            var transform = head.World2BoneInitial * head.TransformMatrix * Node.GetTransform.GlobalTransform;
            headBody.WorldTransform = (transform).Convert();
            //head.SetTransform(Quaternion.FromEulerAngles(0,0.5f * (float)Math.Sin(i*Math.PI/180),0));
            //head.SetTransform(Quaternion.FromEulerAngles(0.5f * (float)Math.Sin(i*Math.PI/180),0,0));
            //head.SetTransform(Quaternion.FromEulerAngles(0,0,0.5f * (float)Math.Sin(i*Math.PI/180)));

            i++;
            
            if (looked)
            {
                var turn = (angle - headSpeed > 0) ? headSpeed : angle;
                angle -= turn;
                //Console.WriteLine(angle);
                var rotation = Quaternion.FromAxisAngle(axis, turn);
                head.SetTransform(head.Rotation * rotation);
            }
            
        }

        void triggerSwitch(Vector3 looker)
        {
            if (looker == Vector3.Zero)
            {
                looked = false;
                Console.WriteLine("dont look");
                /*
                //looker = Vector3.UnitZ;
                var transform = head.TransformMatrix * head.World2BoneInitial * Node.GetTransform.GlobalTransform;
                Vector3 look = new Vector3(transform.M31, transform.M32, transform.M33);
                look.Normalize();
                var lookDest = (head.World2BoneInitial * Node.GetTransform.GlobalTransform * Vector4.UnitZ).Xyz;
                lookDest.Normalize();
                axis = Vector3.Cross(look, lookDest);
                angle = (float)Math.Acos(Vector3.Dot(lookDest, look));
                Console.WriteLine(angle);
                */
            }
            else
            {
                looked = true;

                if (looker != lookTo)
                {
                    lookTo = looker;
                    var transform = head.TransformMatrix * head.World2BoneInitial * Node.GetTransform.GlobalTransform;
                    Vector3 look = -new Vector3(transform.M31, transform.M32, transform.M33);
                    look.Normalize();
                    var lookDest = lookTo - transform.ExtractTranslation();
                    lookDest.Normalize();
                    axis = Vector3.Cross(look, lookDest);
                    //axis.X = 0;
                    angle = (float)Math.Acos(Vector3.Dot(lookDest, look));
                }
            }
        }
    }
}

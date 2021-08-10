using System;
using Toys;
using OpenTK.Mathematics;
using BulletSharp;

namespace ModelViewer
{
    
    class NpcAI : ScriptingComponent
    {
        public bool Busy { get; private set; }
        float headSpeed = (float)(1.7f * Math.PI / 180);
        BoneTransform head;
        int i = 0;
        SphereShape headBox;
        RigidBody headBody;
        bool looked;
        Vector3 lookTo;
        Vector2 angle;
        Vector2 targetAngle;
        Vector2 angSpeed;
        float thetaDef = 0;
        float phiDef = 0;
        float thetaMin = (float)(-60 * Math.PI / 180);
        float thetaMax = (float)(60 * Math.PI / 180);
        float phiMin = (float)(-70 * Math.PI / 180);
        float phiMax = (float)(70 * Math.PI / 180);
        int timer = 180;
        Morph face;
        AudioSource audio;
        void Start()
        {
            var rigged = Node.GetComponent<Animator>();
            if (rigged)
            {
                head = rigged.BoneController.GetBone("頭");
                head.FollowAnimation = false;
            }

            //create head ray collision object
            headBox = new SphereShape(0.35f);
            var rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix4.Identity.Convert()), headBox, BulletSharp.Math.Vector3.Zero);
            headBody = new RigidBody(rbInfo);
            CoreEngine.pEngine.World.AddRigidBody(headBody, (int)CollisionFilleters.Look, (int)CollisionFilleters.Look);
            //headBody.CollisionFlags |= CollisionFlags.KinematicObject;
            //ManifoldPoint.ContactAdded = 
            headBody.UserObject = new Action<Vector3>(triggerSwitch);
            headBody.UserIndex = 1;

            targetAngle = new Vector2(thetaDef,phiDef);
            angle = targetAngle;
            angSpeed = Vector2.Zero;

            audio = Node.AddComponent<AudioSource>();
            audio.SetAudioClip(ResourcesManager.LoadAsset<AudioClip>(@"Assets\Sound\13\voice_120.mp3"));
            var morphes =  Node.GetComponent<MeshDrawer>().Morphes;
            face = Array.Find(morphes,(a) => a.Name == "むみぃ");
        }


        void Update()
        {              
            var transform = head.World2BoneInitial * head.TransformMatrix * Node.GetTransform.GlobalTransform;
            headBody.WorldTransform = transform.Convert();
            //head.SetTransform(Quaternion.FromEulerAngles(0,0.5f * (float)Math.Sin(i*Math.PI/180),0));
            //head.SetTransform(Quaternion.FromEulerAngles(0.5f * (float)Math.Sin(i*Math.PI/180),0,0));
            //head.SetTransform(Quaternion.FromEulerAngles(0,0,0.5f * (float)Math.Sin(i*Math.PI/180)));

            i++;
            if (timer-- < 1)
                triggerSwitch(Vector3.Zero);

            if (targetAngle != Vector2.Zero)
            {

                var angleStep = angSpeed;
                if (Math.Abs(targetAngle.X) < 0.000001f)
                    targetAngle.X = 0;
                if (Math.Abs(targetAngle.Y) < 0.000001f)
                    targetAngle.Y = 0;
                
                if (Math.Abs(targetAngle.X) - Math.Abs(angSpeed.X) < 0 || (Math.Abs(targetAngle.Y) - Math.Abs(angSpeed.Y) < 0))
                    angleStep = targetAngle;
                targetAngle -= angleStep;
                angle += angleStep;

                head.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, angle.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.X));
                
               // head.SetTransform(Quaternion.FromEulerAngles(targetAngle.X, targetAngle.Y, 0));

            }

        }

        void triggerSwitch(Vector3 looker)
        {
            //skip message if script detached
            if (!Node)
                return;

            if (looker == Vector3.Zero)
            {
                looked = false;
                targetAngle.X = thetaDef;
                targetAngle.Y = phiDef;
                CalcRotation();
                lookTo = looker;
                face.MorphDegree = 0f;
            }
            else
            {
                looked = true;
                timer = 300;

                if (lookTo == Vector3.Zero)
                {
                    if (!audio.IsPlaing)
                    {
                        audio.Play();
                    }
                    face.MorphDegree = 1f;
                }
                if (looker != lookTo)
                {
                    lookTo = looker;
                    var transform = head.TransformMatrix * head.World2BoneInitial * Node.GetTransform.GlobalTransform;

                    var lookDest = lookTo - transform.ExtractTranslation();
                    lookDest.Normalize();
                    //phi
                    targetAngle.Y = -(float)Math.Asin(lookDest.X);

                    if (lookDest.Z > 0)
                        targetAngle.Y = (float)Math.PI - targetAngle.Y;

                    if (targetAngle.Y > (float)Math.PI)
                        targetAngle.Y -= 2 * (float)Math.PI;

                    //theta
                    targetAngle.X = (float)Math.Asin(lookDest.Y);

                    //limit rotation
                    if (targetAngle.X < thetaMin)
                        targetAngle.X = thetaMin;
                    else if (targetAngle.X > thetaMax)
                        targetAngle.X = thetaMax;

                    if (targetAngle.Y < phiMin)
                        targetAngle.Y = phiMin;
                    else if (targetAngle.Y > phiMax)
                        targetAngle.Y = phiMax;

                    CalcRotation();

                    

                }

                
            }
        }

        void CalcRotation()
        {
            targetAngle -= angle;
            //diagonal speed calculation
            if (targetAngle.X == 0)
            {
                angSpeed.X = 0;
                angSpeed.Y = Math.Sign(targetAngle.Y) * headSpeed;
            }
            else
            {
                angSpeed.X = (float)Math.Sqrt(Math.Pow(headSpeed, 2) * Math.Pow(targetAngle.X, 2) / (Math.Pow(targetAngle.X, 2) + Math.Pow(targetAngle.Y, 2)));
                angSpeed.Y = angSpeed.X * Math.Abs(targetAngle.Y / targetAngle.X);

                angSpeed.X = Math.Sign(targetAngle.X) * angSpeed.X;
                angSpeed.Y = Math.Sign(targetAngle.Y) * angSpeed.Y;
            }
        }

        protected override void Destroy()
        {
            CoreEngine.pEngine.World.RemoveCollisionObject(headBody);
            headBody.Dispose();
        }
    }
}

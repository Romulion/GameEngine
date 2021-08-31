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
        public RigidBody headBody;
        bool looked;
        SceneNode lookTo;
        Vector2 angle;
        Vector2 targetAngle;
        Vector2 angSpeed;
        float thetaDef = 0;
        float phiDef = 0;
        float thetaMin = -60 * MathF.PI / 180;
        float thetaMax = 60 * MathF.PI / 180;
        float phiMin = -70 * MathF.PI / 180;
        float phiMax = 70 * MathF.PI / 180;
        int timer = 180;
        AudioSource audio;

        NPCFaceController faceController;
        NPCExpression expression;

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
            headBody.UserObject = new Action<SceneNode>(triggerSwitch);
            headBody.UserIndex = 1;
            headBody.SetCustomDebugColor(BulletSharp.Math.Vector3.One);
            targetAngle = new Vector2(thetaDef,phiDef);
            angle = targetAngle;
            angSpeed = Vector2.Zero;

            audio = Node.AddComponent<AudioSource>();
            audio.SetAudioClip(ResourcesManager.LoadAsset<AudioClip>(@"Assets\Sound\13\voice_120.mp3"));
            var meshDrawer =  Node.GetComponent<MeshDrawer>();

            faceController = new NPCFaceController(meshDrawer, rigged);

            expression = new NPCExpression("Smile mumi");
            expression.SetExpression("むみ",1);
            expression.SetExpression("ω", 1);
            //expression.EyeLocationX = -1f;
        }


        void Update()
        {
            faceController.Update();

            var transform = head.World2BoneInitial * head.TransformMatrix * Node.GetTransform.GlobalTransform;
            headBody.WorldTransform = transform.Convert();

            i++;
            if (looked && timer-- == 0)
                triggerSwitch(null);

            if (targetAngle != Vector2.Zero)
            {

                var angleStep = angSpeed;
                if (Math.Abs(targetAngle.Y) < 0.000001f)
                    targetAngle.Y = 0;
                if (Math.Abs(targetAngle.X) < 0.000001f)
                    targetAngle.X = 0;
                
                if (Math.Abs(targetAngle.Y) - Math.Abs(angSpeed.Y) < 0 || (Math.Abs(targetAngle.X) - Math.Abs(angSpeed.X) < 0))
                    angleStep = targetAngle;
                targetAngle -= angleStep;
                angle += angleStep;

                head.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, -angle.X) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.Y));
                
               // head.SetTransform(Quaternion.FromEulerAngles(targetAngle.X, targetAngle.Y, 0));

            }

        }

        void triggerSwitch(SceneNode looker)
        {

            //skip message if script detached
            if (!Node)
                return;

            if (looker == null && lookTo != null)
            {
                looked = false;
                targetAngle.Y = thetaDef;
                targetAngle.X = phiDef;
                CalcRotation();
                lookTo = null;
                faceController.SetDefaultEspression(0.3f);
            }
            else if (looker != null)
            {
                looked = true;
                timer = 300;

                if (lookTo == null)
                {
                    if (!audio.IsPlaing)
                    {
                        audio.Play();
                    }
                    faceController.ChangeEspression(expression, 0.2f);
                }

                if (looker != lookTo)
                {
                    lookTo = looker;
                    var transform = head.TransformMatrix * head.World2BoneInitial * Node.GetTransform.GlobalTransform;
                    var lookDest = lookTo.GetTransform.GlobalTransform.ExtractTranslation() - transform.ExtractTranslation();

                    var lookInit = (new Vector4(0, 0, 1, 1) * transform).Xyz - transform.ExtractTranslation();

                    var angleInit = Toys.MathHelper.ConvertVector2SphereAngles(lookInit);
                    //phi
                    targetAngle.X = MathF.Atan2(lookDest.Z, lookDest.X);
                    //theta
                    targetAngle.Y = MathF.Acos(lookDest.Y / lookDest.Xzy.Length);

                    targetAngle -= angleInit;

                    //limit rotation
                    if (targetAngle.X < -MathF.PI)
                        targetAngle.X += MathF.PI * 2;
                    if (targetAngle.X > MathF.PI)
                        targetAngle.X -= MathF.PI * 2;

                    if (targetAngle.Y < thetaMin)
                        targetAngle.Y = thetaMin;
                    else if (targetAngle.Y > thetaMax)
                        targetAngle.Y = thetaMax;

                    if (targetAngle.X < phiMin)
                        targetAngle.X = phiMin;
                    else if (targetAngle.X > phiMax)
                        targetAngle.X = phiMax;

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

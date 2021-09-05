using System;
using Toys;
using OpenTK.Mathematics;
using BulletSharp;
using System.Collections.Generic;

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
        Dictionary<int, NPCExpression> expressionDict = new Dictionary<int, NPCExpression>();

        SceneNode textBubbleCanvas;
        TextBox textBubble;
        Dictionary<int, Tuple<string, int, int>> dialogData = new Dictionary<int, Tuple<string, int, int>>();
        AudioClip clip;
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
            //headBody.SetCustomDebugColor(BulletSharp.Math.Vector3.One);
            targetAngle = new Vector2(thetaDef,phiDef);
            angle = targetAngle;
            angSpeed = Vector2.Zero;
            audio = Node.AddComponent<AudioSource>();
            //audio.SetAudioClip(ResourcesManager.LoadAsset<AudioClip>(@"Assets\Sound\13\voice_120.mp3"));
            //expression.EyeLocationX = -1f;

            PrepareExpressions(rigged);
            PrepareSpeech();
        }


        void Update()
        {
            textBubbleCanvas.GetTransform.LookAt(CoreEngine.GetCamera.GetPos);

            faceController.Update();
            var transform = head.World2BoneInitial * head.TransformMatrix * Node.GetTransform.GlobalTransform;
            headBody.WorldTransform = transform.Convert();
            i++;
            if (looked && timer-- == 0)
                triggerSwitch(null);
            if (CoreEngine.time.FrameCount % 10 == 0 && looked)
                HeadTrack();

            if (targetAngle != angle)
            {
                var angleStep = targetAngle - angle;

                if (MathF.Abs(angleStep.X) > MathF.Abs(angSpeed.X))
                    angleStep.X = angSpeed.X;
                if (MathF.Abs(angleStep.Y) > MathF.Abs(angSpeed.Y))
                    angleStep.Y = angSpeed.Y;
                angle += angleStep;
                head.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, -angle.X) * Quaternion.FromAxisAngle(Vector3.UnitX, angle.Y));
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
                        PlayRandomVoice();
                    }
                    faceController.ChangeEspression(expressionDict[4], 0.2f);
                }

                if (looker != lookTo)
                {
                    lookTo = looker;
                    HeadTrack();
                }

                
            }
        }

        void HeadTrack()
        {
            var transform = head.World2BoneInitial * Node.GetTransform.GlobalTransform;
            var lookDest = lookTo.GetTransform.GlobalTransform.ExtractTranslation() - transform.ExtractTranslation();

            var lookInit = (new Vector4(0, 0, 1, 1) * transform).Xyz - transform.ExtractTranslation();

            var angleInit = Toys.MathHelper.ConvertVector2SphereAngles(lookInit);
            //phi
            targetAngle.X = MathF.Atan2(lookDest.Z, lookDest.X);
            //theta
            targetAngle.Y = MathF.Acos(lookDest.Y / lookDest.Xzy.Length);

            targetAngle -= angleInit;



            //limit rotation
            if (targetAngle.X > MathF.PI)
                targetAngle.X -= 2 * MathF.PI;
            else if (targetAngle.X < -MathF.PI)
                targetAngle.X += 2 * MathF.PI;

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

        void CalcRotation()
        {
            var angleDelta = targetAngle - angle;

            //diagonal speed calculation
            if (angleDelta.X == 0)
            {
                angSpeed.X = 0;
                angSpeed.Y = Math.Sign(angleDelta.Y) * headSpeed;
            }
            else
            {
                angSpeed.X = (float)Math.Sqrt(Math.Pow(headSpeed, 2) * Math.Pow(angleDelta.X, 2) / (Math.Pow(angleDelta.X, 2) + Math.Pow(angleDelta.Y, 2)));
                angSpeed.Y = angSpeed.X * Math.Abs(angleDelta.Y / angleDelta.X);

                angSpeed.X = Math.Sign(angleDelta.X) * angSpeed.X;
                angSpeed.Y = Math.Sign(angleDelta.Y) * angSpeed.Y;
            }
        }

        void PrepareExpressions(Animator rigged)
        {
            var meshDrawer = Node.GetComponent<MeshDrawer>();
            faceController = new NPCFaceController(meshDrawer, rigged);

            var expression = new NPCExpression("Default");
            expressionDict.Add(1, expression);

            expression = new NPCExpression("Smile");
            expression.SetExpression("笑い", 0.7f);
            expression.SetExpression("照れ", 1);
            expressionDict.Add(2, expression);

            expression = new NPCExpression("Aspire");
            expression.SetExpression("びっくり2", 1f);
            expression.SetExpression("HL無し", 1);
            expression.SetExpression("ハート目3", 1);
            expression.SetExpression("デフォ", 1);
            expression.SetExpression("照れ", 1);
            expression.SetExpression("照れ2", 0.9f);
            expressionDict.Add(3, expression);

            expression = new NPCExpression("Blush");
            expression.SetExpression("にこり", 1f);
            expression.SetExpression("照れ", 1);
            expression.SetExpression("照れ2", 0.9f);
            expression.SetExpression("まばたき", 0.3f);
            expressionDict.Add(4, expression);

            expression = new NPCExpression("Atronished");
            expression.SetExpression("びっくり2", 1f);
            expression.SetExpression("瞳小", 0.3f);
            expression.SetExpression("むぅ", 1f);
            expression.SetExpression("照れ", 1);
            expressionDict.Add(5, expression);

            expression = new NPCExpression("Fuu");
            expression.SetExpression("困る", 1f);
            expression.SetExpression("まばたき", 0.3f);
            expression.SetExpression("ぷくー", 0.9f);
            expressionDict.Add(6, expression);

            expression = new NPCExpression("Not");
            expression.SetExpression("怒りみけん", 1f);
            expression.SetExpression("むみ", 1f);
            expression.SetExpression("むぅ", 1f);
            expression.SetExpression("照れ2", 0.9f);
            expression.SetExpression("照れ", 1);
            expressionDict.Add(7, expression);

            expression = new NPCExpression("Tired");
            expression.SetExpression("ジト目", 1f);
            expression.SetExpression("無表情", 1f);
            expression.SetExpression("汗右", 1f);
            expressionDict.Add(8, expression);

            expression = new NPCExpression("Cry");
            expression.SetExpression("悲しい", 1);
            expression.SetExpression("涙2", 1f);
            expression.SetExpression("うるうる", 1f);
            expression.SetExpression("まばたき", 0.3f);
            expression.SetExpression("無表情", 1f);
            //expression.SetExpression("照れ", 1);
            expressionDict.Add(9, expression);

            expression = new NPCExpression("Exhausterd");
            expression.SetExpression("悲しい", 1);
            expression.SetExpression("汗右", 1f);
            expression.SetExpression("汗", 1f);
            expression.SetExpression("半目", 1f);
            expression.SetExpression("無表情", 1f);
            //expression.SetExpression("照れ", 1);
            expressionDict.Add(10, expression);

        }

        public void PlayRandomVoice()
        {
            var randomizer = new Random();
            var num = randomizer.Next(1, 300);

            if (clip)
                ResourcesManager.DeleteResource(clip);
            clip = ResourcesManager.LoadAsset<AudioClip>(@"Assets\Sound\13\voice_" + num.ToString().PadLeft(3, '0') + ".mp3");
            audio.SetAudioClip(clip);
            audio.Play();
            faceController.ChangeEspression(expressionDict[dialogData[num].Item2], 0.2f);
            textBubble.SetText(dialogData[num].Item1);
        }
        void PrepareSpeech()
        {
            var reader = System.IO.File.ReadLines(@"Assets\Sound\13\voice_text_13.txt");
            foreach(var line in reader)
            {
                var parts = line.Split(',');
                dialogData.Add(int.Parse(parts[0]), new Tuple<string, int, int>(parts[1].Replace("<br>","\n"), int.Parse(parts[2]), int.Parse(parts[3])));
            }

            
            textBubbleCanvas = new SceneNode();
            textBubbleCanvas.GetTransform.Position = new Vector3(0, 1.8f, 0);
            textBubbleCanvas.SetParent(Node);
            var textboxCanvas = textBubbleCanvas.AddComponent<Canvas>();
            var root = new UIElement();
            textboxCanvas.Add2Root(root);
            root.AddComponent<RawImage>();

            textBubble = (TextBox)root.AddComponent<TextBox>();
            var rect = root.GetTransform;
            rect.anchorMax = new Vector2(0, 0);
            rect.anchorMin = new Vector2(0, 0);
            rect.offsetMin = new Vector2(-100, -30);
            rect.offsetMax = new Vector2(100, 30);
            textBubble.SetText("");
            textBubble.textCanvas.colour = Vector3.Zero;
            textBubble.textCanvas.alignVertical = TextAlignVertical.Center;
            textBubble.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            textBubble.textCanvas.Scale = 0.5f;
            textboxCanvas.Mode = Canvas.RenderMode.WorldSpace;
            textboxCanvas.Canvas2WorldScale = 0.0025f;
            
        }
        protected override void Destroy()
        {
            CoreEngine.pEngine.World.RemoveCollisionObject(headBody);
            headBody.Dispose();
        }
    }
}

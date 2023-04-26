using System;
using Toys;
using OpenTK.Mathematics;
using BulletSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ModelViewer
{

    class NpcAI
    {
        public bool Busy { get; private set; }
        BoneTransform head;
        SphereShape headBox;
        public RigidBody headBody;
        int timer = 180;
        AudioSource audio;
        public NPCFaceController FaceController { get; private set; }
        public NPCHeadController HeadController { get; private set; }
        public NPCBreatheControll BreatheControll { get; private set; }
        Dictionary<int, NPCExpression> expressionDict = new Dictionary<int, NPCExpression>();

        SceneNode textBubbleCanvas;
        Canvas textboxCanvas;
        TextBox textBubble;
        readonly Dictionary<int, Tuple<string, int, int>> dialogData = new Dictionary<int, Tuple<string, int, int>>();
        //AudioClip clip;
        Action onClipLoad;

        SceneNode CharNode;
        public NpcAI(SceneNode charNode)
        {
            CharNode = charNode;
            var rigged = CharNode.GetComponent<Animator>();
            audio = CharNode.AddComponent<AudioSource>();

            HeadController = new NPCHeadController(charNode);
            BreatheControll = new NPCBreatheControll(rigged) {IsBreathing = true };
            //collider
            SetupHeadCollider(rigged);
            PrepareExpressions(rigged);
            PrepareSpeechBubble();
        }


        internal void Update()
        {
            if (onClipLoad != null)
            {
                textboxCanvas.IsActive = true;
                onClipLoad.Invoke();
                onClipLoad = null;
            }

            if (textboxCanvas.IsActive)
                textBubbleCanvas.GetTransform.LookAt(CoreEngine.GetCamera.GetPos);

            FaceController.Update();
            HeadController.Update();
            BreatheControll.Update();
            //collider
            //Update head collider

            var transform = head.World2BoneInitial * head.TransformMatrix * CharNode.GetTransform.GlobalTransform;
            headBody.WorldTransform = transform.Convert();
            if (HeadController.IsTrackTarget && timer-- == 0)
                triggerSwitch(null);
            

            if (textboxCanvas.IsActive && !audio.IsPlaing)
                textboxCanvas.IsActive = false;
        }

        void triggerSwitch(SceneNode looker)
        {
            //skip message if script detached
            if (!CharNode)
                return;
            /*
            if (looker == null && headController.LookTarget != null)
            {
                headController.IsTrackTarget = false;
                headController.Return2Base();
                headController.LookTarget = null;
                FaceController.SetDefaultEspression(0.3f);
            }
            else if (looker != null)
            {
                headController.IsTrackTarget = true;
                timer = 300;

                if (headController.LookTarget == null)
                {
                    if (!audio.IsPlaing)
                    {
                        //PlayRandomVoice();
                    }
                    FaceController.ChangeEspression(expressionDict[1], 0.4f);
                }

                if (looker != headController.LookTarget)
                {
                    headController.LookTarget = looker;
                    headController.HeadTrack();
                }
            }
            */
        }

        void SetupHeadCollider(Animator rigged)
        {
            if (rigged)
                head = rigged.BoneController.GetBone("頭");
            //create head ray collision object
            headBox = new SphereShape(0.35f);
            var rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(Matrix4.Identity.Convert()), headBox, BulletSharp.Math.Vector3.Zero);
            headBody = new RigidBody(rbInfo);
            CoreEngine.PhysEngine.World.AddRigidBody(headBody, (int)CollisionFilleters.Look, (int)CollisionFilleters.Look);
            //headBody.CollisionFlags |= CollisionFlags.KinematicObject;
            headBody.UserObject = new Action<SceneNode>(triggerSwitch);
            headBody.UserIndex = 1;
        }
        void PrepareExpressions(Animator rigged)
        {
            var meshDrawer = CharNode.GetComponent<MeshDrawerRigged>();
            FaceController = new NPCFaceController(meshDrawer, rigged, audio);
            FaceController.IsLipSync = true;

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
            expression.SetExpression("汗", 1f);
            expression.SetExpression("半目", 1f);
            expression.SetExpression("無表情", 1f);
            //expression.SetExpression("照れ", 1);
            expressionDict.Add(10, expression);

            expression = new NPCExpression("Smug");
            expression.SetExpression("ω", 1);
            expression.SetExpression("ジト目", 1);
            expressionDict.Add(11, expression);

            expression = new NPCExpression("Broken");
            expression.SetExpression("HL無し", 1);
            expression.SetExpression("無表情", 1f);
            expressionDict.Add(12, expression);

            expression = new NPCExpression("Terrified");
            expression.SetExpression("瞳小", 1);
            expression.SetExpression("無表情", 1f);
            expressionDict.Add(13, expression);
        }

        public void PlayRandomVoice()
        {
            var randomizer = new Random();
            var num = randomizer.Next(1, 198);

            PlayVoice(num);
        }

        public void PlayVoice(int ID)
        {
            /*
            if (clip)
            {
                ResourcesManager.DeleteResource(clip);
                clip = null;
            }
            */
            var clipTask = ResourcesManager.LoadAssetAsync<AudioClip>(@"Assets\Sound\13\voice_" + ID.ToString().PadLeft(3, '0') + ".mp3");
            clipTask.ContinueWith((clipT) =>
           {
               onClipLoad += () => {

                   audio.SetAudioClip(clipT.Result);
                   audio.Play();
                   FaceController.ChangeEspression(expressionDict[dialogData[ID].Item2], 0.2f);
                   textBubble.Text = dialogData[ID].Item1;
               };
           });
        }

        public NPCExpression[] GetExpressions()
        {
            return expressionDict.Values.ToArray();
        }

        public void SetExpression(NPCExpression expression)
        {
            FaceController.ChangeEspression(expression, 0.2f);
        }

        void PrepareSpeechBubble()
        {
            var reader = System.IO.File.ReadLines(@"Assets\Sound\13\voice_text_13.txt");
            foreach(var line in reader)
            {
                var parts = line.Split(',');
                dialogData.Add(int.Parse(parts[0]), new Tuple<string, int, int>(parts[1].Replace("<br>","\n"), int.Parse(parts[2]), int.Parse(parts[3])));
            }

            
            textBubbleCanvas = new SceneNode();
            textBubbleCanvas.Name = "TextBubble";
            textBubbleCanvas.GetTransform.Position = new Vector3(0, 1.8f, 0);
            textBubbleCanvas.SetParent(CharNode);
            textboxCanvas = textBubbleCanvas.AddComponent<Canvas>();
            var root = new UIElement();
            textboxCanvas.Add2Root(root);
            root.AddComponent<RawImage>();

            textBubble = (TextBox)root.AddComponent<TextBox>();
            var rect = root.GetTransform;
            rect.anchorMax = new Vector2(0, 0);
            rect.anchorMin = new Vector2(0, 0);
            rect.offsetMin = new Vector2(-100, -30);
            rect.offsetMax = new Vector2(100, 30);
            textBubble.Text = "";
            textBubble.textCanvas.colour = Vector3.Zero;
            textBubble.textCanvas.alignVertical = TextAlignVertical.Center;
            textBubble.textCanvas.alignHorizontal = TextAlignHorizontal.Center;
            textBubble.textCanvas.Scale = 0.5f;
            textboxCanvas.Mode = Canvas.RenderMode.WorldSpace;
            textboxCanvas.Canvas2WorldScale = 0.0025f;

            textboxCanvas.IsActive = false;
        }
        internal void Destroy()
        {
            CoreEngine.PhysEngine.World.RemoveCollisionObject(headBody);
            headBody.Dispose();
        }
    }
}

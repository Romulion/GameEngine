using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    /// <summary>
    /// Controlls Face Expressions and eye direction
    /// </summary>
    class NPCFaceController
    {
        Animator anim;
        MeshDrawer mesh;
        BoneTransform eyesBone;
        BoneTransform headBone;
        Morph[] morphes;
        
        AudioSource voice;
        public bool IsLipSync = false;
        public bool IsBlinking = true;
        public NPCExpression GetExpression { get; private set; }
        public NPCExpression DefaultExpression { get; set; }
        private Dictionary<Morph, float> changeMorphs = new Dictionary<Morph, float>();
        float timer;
        public float EyeLocationX { get; private set; }
        public float EyeLocationY { get; private set; }
        //eyes rotation range
        const float rotXMax = 10f / 180 * MathF.PI, rotYMax = 12f / 180 * MathF.PI;
        Morph mouth;
        Morph eyes;
        Random blinkInterval;
        public bool IsCurrentBlink { get; private set; }
        float blikProgress = 0;
        float lastBlinked = 0;
        int nextBlinkIn = 0;
        const float blinkTime = 0.3f;
        Vector2 targetAngle;
        Vector2 angle;
        Vector2 startAngle;
        float rotationProgress = 0;
        BezierCurveCubic eyeMoveCurve;
        SceneNode target;
        public NPCFaceController(MeshDrawer meshDrawer, Animator animator, AudioSource audio)
        {
            anim = animator;
            mesh = meshDrawer;
            voice = audio;
            InitialSetup();
        }

        /// <summary>
        /// Prepare  references
        /// </summary>
        void InitialSetup()
        {
            headBone = anim.BoneController.GetBone("頭");
            eyesBone = anim.BoneController.GetBone("両目");
            eyesBone.FollowAnimation = false;
            
            morphes = mesh.Morphes.ToArray();
            EyeLocationX = 0;
            EyeLocationY = 0;
            DefaultExpression = new NPCExpression("Def");

            mouth = morphes.First((m) => m.Name == "お");
            eyes = morphes.First((m) => m.Name == "まばたき");

            blinkInterval = new Random();
            nextBlinkIn = blinkInterval.Next(2, 10);

            eyeMoveCurve = new BezierCurveCubic(Vector2.Zero, Vector2.One, new Vector2(0f, 1), new Vector2(0, 1));


            target = CoreEngine.GetCamera.Node;
        }

        internal void Update()
        {

            if (timer > 0)
                PerformExpressionChange(CoreEngine.FrameTimer.FrameTime);


            if (voice.IsPlaing && IsLipSync && CoreEngine.Time.FrameCount % 3 == 0)
                mouth.MorphDegree = voice.GetCurrentVolume();


            if (IsBlinking && !IsCurrentBlink)
            {
                lastBlinked += CoreEngine.FrameTimer.FrameTime;
                if (nextBlinkIn <= lastBlinked)
                {
                    IsCurrentBlink = true;
                    nextBlinkIn = blinkInterval.Next(2, 10);
                    lastBlinked = 0;
                }
                    
            }

            
            if (IsCurrentBlink)
            {
                
                if (blikProgress < blinkTime)
                    blikProgress += CoreEngine.FrameTimer.FrameTime;
                else
                {
                    blikProgress = 0;
                    IsCurrentBlink = false;
                } 
                eyes.MorphDegree = MathF.Sin(MathF.PI / blinkTime * blikProgress);
            }
            

            
            if (targetAngle != angle)
            {

                rotationProgress += CoreEngine.FrameTimer.FrameTime / 0.3f;

                if (rotationProgress > 1)
                    rotationProgress = 1;


                angle = startAngle + (targetAngle - startAngle) * eyeMoveCurve.CalculatePoint(rotationProgress).Y;

                eyesBone.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, -angle.X) * Quaternion.FromAxisAngle(Vector3.UnitX, -angle.Y));

            }
            
            //EyeLookAt(target);
            //eyesBone.SetTransform(Quaternion.FromAxisAngle(Vector3.UnitY, -targetAngle.X) * Quaternion.FromAxisAngle(Vector3.UnitX, -targetAngle.Y));

        }

        public void ResetEyeDirection()
        {
            targetAngle.Y = 0;
            targetAngle.X = 0;
            CalcRotation();
        }

        public void LookAt(SceneNode LookTarget)
        {
            /*
            var dest = new Vector3(-1,0,0);
            var dead = Matrix4.CreateRotationY(-MathF.PI / 2);
            dead.M42 = 1;

            dest = (dead * new Vector4(dest, 1)).Xyz;
            Console.WriteLine(Toys.MathHelper.ConvertVector2SphereAngles(dest));
            */
            var headBase = eyesBone.InitialLocalTransform;
            headBase.M42 -= 0.17f;
            headBase.M43 -= 0.08f;
            headBase *= eyesBone.Parent.TransformMatrix * anim.Node.GetTransform.GlobalTransform;
            var headCurr = eyesBone.World2BoneInitial;
            headCurr.M42 -= 0.17f;
            headCurr.M43 -= 0.08f;
            headCurr *= eyesBone.TransformMatrix * anim.Node.GetTransform.GlobalTransform;
            var lookDest = LookTarget.GetTransform.GlobalTransform.ExtractTranslation() - headCurr.ExtractTranslation();
            //Console.WriteLine(lookDest);
            lookDest = (headBase * new Vector4(lookDest, 1)).Xyz;
            //Console.WriteLine(lookDest);
            targetAngle = Toys.MathHelper.ConvertVector2SphereAngles(lookDest);


            //compensate eye position
            if (targetAngle.X < 0)
                targetAngle.X = -MathF.Pow(targetAngle.X,2);
            else
                targetAngle.X = MathF.Pow(targetAngle.X, 2);

            if (targetAngle.Y < 0)
                targetAngle.Y = -MathF.Pow(targetAngle.Y, 2);
            else
                targetAngle.Y = MathF.Pow(targetAngle.Y, 2);

            //limit rotation
            if (targetAngle.Y < -rotYMax)
                targetAngle.Y = -rotYMax;
            else if (targetAngle.Y > rotYMax)
                targetAngle.Y = rotYMax;

            if (targetAngle.X < -rotXMax)
                targetAngle.X = -rotXMax;
            else if (targetAngle.X > rotXMax)
                targetAngle.X = rotXMax;

            //targetAngle -= angleInit;
            CalcRotation();
        }
        void CalcRotation()
        {
            rotationProgress = 0;
            startAngle = angle;
        }
        public void PerformEyeBlink()
        {
            if (!IsCurrentBlink)
            {
                IsCurrentBlink = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transit"> transit time in seconds</param>
        public void ChangeEspression(NPCExpression expression, float transit = 0)
        {
            changeMorphs.Clear();
            var oldExpression = GetExpression;
            GetExpression = expression.Clone();

            if (oldExpression != null)
                foreach (var expr in oldExpression.expressionList)
                {
                    //reset morph to zero
                    if (!GetExpression.expressionList.ContainsKey(expr.Key))
                        GetExpression.SetExpression(expr.Key, 0);

                }
            /*
            //get values to change
            foreach (var morph in morphes)
            {
                if (morph.MorphDegree != 0) 
                {
                    //reset morph to zero
                    if (!GetExpression.expressionList.ContainsKey(morph.Name))
                        GetExpression.SetExpression(morph.Name,0);
                }

            }
           */

            //get morph pointers
            foreach (var morphName in GetExpression.expressionList.Keys)
            {
                var morph = Array.Find(morphes, (a) => a.Name == morphName);
                if (morph != null)
                {
                    changeMorphs.Add(morph, GetExpression.expressionList[morphName]);
                }
                else
                    Logger.Warning(String.Format("Morph {0} not found in model", morphName));
            }

            
            if (transit > 0)
            {
                timer = transit;
            }
            else
            {
                timer = 1;
                PerformExpressionChange(1);
            }

        }
        

        void PerformExpressionChange(float progression)
        {
            if (progression > timer)
                progression = timer;
            var degree = progression/ timer;
            foreach (var morph in changeMorphs.Keys)
            {
                morph.MorphDegree += (changeMorphs[morph] - morph.MorphDegree) * degree;
            }

            EyeLocationX += (GetExpression.EyeLocationX - EyeLocationX) * degree;
            EyeLocationY += (GetExpression.EyeLocationY - EyeLocationY) * degree;
            eyesBone.SetTransform(new Quaternion(EyeLocationY * rotYMax, EyeLocationX * rotXMax, 0));
            timer -= progression;
        }

        public void SetDefaultEspression(float transit = 0)
        {
            ChangeEspression(DefaultExpression, transit);
        }
    }
}

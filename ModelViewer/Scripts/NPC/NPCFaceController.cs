using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    class NPCFaceController
    {
        Logger logger = new Logger("NPCFaceController");
        Animator anim;
        MeshDrawer mesh;
        BoneTransform eyesBone;
        Morph[] morphes;
        AudioSource voice;
        public NPCExpression GetExpression { get; private set; }
        public NPCExpression DefaultExpression { get; set; }
        private Dictionary<Morph, float> changeMorphs = new Dictionary<Morph, float>();
        float timer;
        public float EyeLocationX { get; private set; }
        public float EyeLocationY { get; private set; }
        //eyes rotation range
        const float rotXMax = 10f / 180 * MathF.PI, rotYMax = -10f / 180 * MathF.PI;

        public NPCFaceController(MeshDrawer meshDrawer, Animator animator)
        {
            anim = animator;
            mesh = meshDrawer;
            InitialSetup();
        }

        /// <summary>
        /// Prepare  references
        /// </summary>
        void InitialSetup()
        {
            eyesBone = anim.BoneController.GetBone("両目");
            eyesBone.FollowAnimation = false;
            morphes = mesh.Morphes;
            EyeLocationX = 0;
            EyeLocationY = 0;
            DefaultExpression = new NPCExpression("Def");
        }

        internal void Update()
        {

            if (timer > 0)
                PerformExpressionChange(CoreEngine.frameTimer.FrameTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transit"> transit time in seconds</param>
        public void ChangeEspression(NPCExpression expression, float transit = 0)
        {
            changeMorphs.Clear();

            GetExpression = expression.Clone();

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

            //get morph pointers
            foreach (var morphName in GetExpression.expressionList.Keys)
            {
                var morph = Array.Find(morphes, (a) => a.Name == morphName);
                if (morph != null)
                {
                    changeMorphs.Add(morph, GetExpression.expressionList[morphName]);
                }
                else
                    logger.Warning(String.Format("Morph {0} not found in model", morphName));
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

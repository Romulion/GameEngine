using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class AnimationController : Component
    {
        AnimationNode context;
        List<AnimationNode> animationList;
        public Animator TargetAnimator;
        bool changed;

        Dictionary<string, bool> boolDict;
        Dictionary<string, float> floatDict;
        Dictionary<string, int> intDict;


        public AnimationController() : base(typeof(AnimationController))
        {
            animationList = new List<AnimationNode>();
        }

        public void SetBool(string key, bool value)
        {
            if (boolDict.ContainsKey(key))
                boolDict[key] = value;
            else
                boolDict.Add(key,value);
            changed = true;
        }

        public void SetFloat(string key, float value)
        {
            if (floatDict.ContainsKey(key))
                floatDict[key] = value;
            else
                floatDict.Add(key, value);
            changed = true;
        }

        public void SetInt(string key, int value)
        {
            if (intDict.ContainsKey(key))
                intDict[key] = value;
            else
                intDict.Add(key, value);
            changed = true;
        }

        internal void Update()
        {
            //check transition
            if (changed)
            {
                changed = false;
                for (int i = 0; i < context.Transitions.Count; i++)
                {
                    if (context.Transitions[i].Condition(this))
                    {
                        UpdateContext(context.Transitions[i].TargetAnimation);
                        return;
                    }
                }
            }

            //check next animation
            if (!context.Repeat && context.NextAnimation != null)
            {
                if (TargetAnimator.IsEnded)
                    UpdateContext(context.NextAnimation);
            }
        }


        void UpdateContext(AnimationNode newContext) 
        {
            context = newContext;
            TargetAnimator.AnimationData = context.MainAnimation;
        }

        internal override void Unload()
        {

        }

        internal override void AddComponent(SceneNode node)
        {
            CoreEngine.animEngine.controllers.Add(this);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.animEngine.controllers.Remove(this);
        }
    }
}

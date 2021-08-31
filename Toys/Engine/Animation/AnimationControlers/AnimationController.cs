﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class AnimationController
    {
        AnimationNode context;
        List<AnimationNode> animationList;
        Animator animator;
        public Animator TargetAnimator
        {
            get { return animator; }
            set
            {
                animator = value;
                UpdateContext(context);
            }
        }
        bool changed;

        Dictionary<string, bool> boolDict;
        Dictionary<string, float> floatDict;
        Dictionary<string, int> intDict;


        public AnimationController(AnimationNode animation)
        {
            context = animation;
            animationList = new List<AnimationNode>();
            AddAnimation(animation);

            boolDict = new Dictionary<string, bool>();
            floatDict = new Dictionary<string, float>();
            intDict = new Dictionary<string, int>();
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

        public bool GetBool(string key)
        {
            if (boolDict.ContainsKey(key))
                return boolDict[key];
            else
                return false;
        }

        public float GetFloat(string key)
        {
            if (floatDict.ContainsKey(key))
                return floatDict[key];
            else
                return 0f;
        }

        public int GetInt(string key)
        {
            if (intDict.ContainsKey(key))
                return intDict[key];
            else
                return 0;
        }

        public AnimationNode FindNode(string name)
        {
            return animationList.Find(n => n.Name == name);
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

        /// <summary>
        /// Set current animation and its transition
        /// </summary>
        public void SetEntry(AnimationNode node)
        {
            context = node;
            AddAnimation(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void AddAnimation(AnimationNode node)
        {
            if (!animationList.Contains(node))
            animationList.Add(node);
        }

        public void RemoveAnimation(AnimationNode node)
        {
            if (animationList.Contains(node))
                animationList.Remove(node);
        }

        /// <summary>
        /// update current animation ant transitions
        /// </summary>
        /// <param name="newContext"></param>
        void UpdateContext(AnimationNode newContext) 
        {
            context = newContext;
            TargetAnimator.AnimationData = context.MainAnimation;
            TargetAnimator.Play();
            TargetAnimator.IsRepeat = context.Repeat;
        }
    }
}

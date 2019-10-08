﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Toys
{

    internal class ScriptingEngine
    {
        List<ScriptingComponent> scripts = new List<ScriptingComponent>();

        Queue<ScriptingComponent> awakeQueue = new Queue<ScriptingComponent>();
        Queue<ScriptingComponent> startQueue = new Queue<ScriptingComponent>();
        List<Message> updates = new List<Message>();
        List<Message> prerender = new List<Message>();
        List<Message> postRender = new List<Message>();


        internal ScriptingEngine()
        {

        }

        internal void AddScript(ScriptingComponent sc)
        {
            scripts.Add(sc);
            awakeQueue.Enqueue(sc);
        }

        internal void Awake()
        {
            while (awakeQueue.Count > 0)
            {
                ScriptingComponent sc = awakeQueue.Dequeue();
                var act = GetMessage(sc, "Awake");
                if (act != null)
                {
                    act.Invoke();
                } 
                sc.IsInstalized = true;
                startQueue.Enqueue(sc);
            }
        }

        internal void Start()
        {
            while (startQueue.Count > 0)
            {
                ScriptingComponent sc = startQueue.Dequeue();
                var act = GetMessage(sc, "Start");
                if (act != null)
                {
                    act.Invoke();
                }

                var mess = GetMessage(sc,"Update");
                if (mess != null)
                    updates.Add(new Message(sc,mess));
                mess = GetMessage(sc, "PreRender");
                if (mess != null)
                    prerender.Add(new Message(sc, mess));

                mess = GetMessage(sc, "PostRender");
                if (mess != null)
                    postRender.Add(new Message(sc, mess));
            }
        }

        Action GetMessage(ScriptingComponent sc, string name)
        {
            MethodInfo method = sc.Type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Action message = null;
            if (method != null && method.GetGenericArguments().Length == 0)
            {
                message = (Action)Delegate.CreateDelegate(typeof(Action), sc, method);
            }

            return message;
        }

        internal void RemoveScript(ScriptingComponent sc)
        {
            scripts.Remove(sc);
            updates.RemoveAll((m) => m.ScriptingObject == sc);
            prerender.RemoveAll((m) => m.ScriptingObject == sc);
        }

        internal void Update()
        {
            foreach (var upd in updates)
                upd.Method();
        }

        internal void PreRender()
        {
            foreach (var prr in prerender)
                prr.Method();
        }

        internal void PostRender()
        {
            foreach (var prr in postRender)
                prr.Method();
        }
    }
}

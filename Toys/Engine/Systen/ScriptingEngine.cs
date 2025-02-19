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
        List<Message> preRender = new List<Message>();
        List<Message> postRender = new List<Message>();
        List<Message> onDestroy = new List<Message>();


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
                act?.Invoke();

                var mess = GetMessage(sc,"Update");
                if (mess != null)
                    updates.Add(new Message(sc,mess));
                mess = GetMessage(sc, "PreRender");
                if (mess != null)
                    preRender.Add(new Message(sc, mess));

                mess = GetMessage(sc, "PostRender");
                if (mess != null)
                    postRender.Add(new Message(sc, mess));
                
                /*
                mess = GetMessage(sc, "OnDestroy");
                if (mess != null)
                    onDestroy.Add(new Message(sc, mess));
                */
            }
        }

        Action GetMessage(ScriptingComponent sc, string name)
        {
            MethodInfo method = sc.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
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

            //Enum safe script removal
            onDestroy.Add(new Message(sc, () =>
           {
               updates.RemoveAll((m) => m.ScriptingObject == sc);
               preRender.RemoveAll((m) => m.ScriptingObject == sc);
               postRender.RemoveAll((m) => m.ScriptingObject == sc);
           }));
        }

        //Main update method
        internal void Update()
        {
            foreach (var upd in updates)
                upd.Method();
        }

        //For processing directly before render sequence started
        internal void PreRender()
        {
            foreach (var prr in preRender)
                prr.Method();
        }

        //For processing directly after render sequence ended
        internal void PostRender()
        {
            foreach (var prr in postRender)
                prr.Method();
        }

        //For enum safe script removing
        internal void Destroy()
        {
            foreach (var prr in onDestroy)
                prr.Method();

            onDestroy.Clear();
        }
    }
}

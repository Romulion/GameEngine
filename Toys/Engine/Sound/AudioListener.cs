﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// can have only one instance
    /// </summary>
    public class AudioListener : Component
    {
        static AudioListener listener;
        public Vector3 direction;
        private AudioListener()
        {
            
        }

        public static AudioListener GetListener()
        {
            if (!listener)
                listener = new AudioListener();
            return listener;
        }

        internal void Update()
        {
            var pos = Node.GetTransform.GlobalPosition;
            var up = Node.GetTransform.Up;
            direction = Node.GetTransform.Forward;
            AL.Listener(ALListener3f.Position, ref pos);
            AL.Listener(ALListenerfv.Orientation, ref direction, ref up);
        }

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.AudioEngine.Listner = this;
        }

        internal override void RemoveComponent()
        {
            Node = null;
            if (CoreEngine.AudioEngine.Listner == this)
                CoreEngine.AudioEngine.Listner = null;
        }
        protected override void Unload()
        {
        }
    }
}
